using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.BlockStore;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Extensions;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Adapters;

namespace Zorbit.Features.Observatory.TableStorage.Indexing.Tasks
{
    public class AddressTask : IndexerTableTask
    {
        private readonly Network _network;
        private readonly IBlockRepository _blockRepository;
        private static readonly ConcurrentDictionary<string, AddressSummaryAdapter> _summaryAdapters = new ConcurrentDictionary<string, AddressSummaryAdapter>();

        public AddressTask(
            Network network,
            IBlockRepository blockRepository,
            AzureStorageClient storageClient,
            IndexerSettings settings)
            : base(storageClient, settings)
        {
            _network = network;
            _blockRepository = blockRepository;
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetAddressTable();
        }
        
        protected override async Task<IEnumerable<ITaskAdapter>> GetTasksAsync(IEnumerable<IBlockInfo> blocks)
        {
            var txModelTasks = await GetTransactionModelsAsync(blocks).ConfigureAwait(false);

            var addresses = txModelTasks
                .Select(t => t.Address)
                .Distinct()
                .ToList();

            var txGroups = txModelTasks
                .OrderBy(t => t.BlockHeight)
                .GroupBy(t => t.Address);

            var result = new List<ITaskAdapter>();

            var summaries = await GetAddressSummariesAsync(addresses).ConfigureAwait(false);

            foreach (var group in txGroups)
            {
                var summary = summaries.FirstOrDefault(s => s.Address.Equals(group.Key));
                result.AddRange(GetAddressAdapters(summary, group.GetEnumerator()));
            }

            result.AddRange(summaries);

            //var duplicates = result.GroupBy(a => new { a.PartitionKey, a.RowKey }).Where(g => g.Count() > 1).ToList();
            //if (duplicates.Any())
            //{
            //}

            return result;
        }

        protected async Task<List<AddressTransactionModel>> GetTransactionModelsAsync(IEnumerable<IBlockInfo> blocks)
        {
            var transactions = new List<AddressTransactionModel>();
            foreach (var block in blocks)
            {
                foreach (var tx in block.Block.Transactions)
                {
                    if (tx.IsCoinBase)
                    {
                        transactions.AddRange(await GetCoinBaseAsync(block, tx).ConfigureAwait(false));
                    }
                    else if (tx.IsCoinStake)
                    {
                        transactions.AddRange(await GetCoinStakeAsync(block, tx).ConfigureAwait(false));
                    }
                    else
                    {
                        transactions.AddRange(await GetTransferAsync(block, tx).ConfigureAwait(false));
                    }
                }
            }
            return transactions;
        }

        private async Task<ICollection<AddressSummaryAdapter>> GetAddressSummariesAsync(ICollection<string> addresses)
        {
            var result = new List<AddressSummaryAdapter>(addresses.Count);
            foreach (var address in addresses)
            {
                if (_summaryAdapters.TryGetValue(address, out var adapter))
                {
                    result.Add(adapter);
                }
                else
                {
                    adapter = await GetAddressSummaryAsync(address).ConfigureAwait(false);
                    _summaryAdapters.TryAdd(address, adapter);
                    result.Add(adapter);
                }
            }
            return result;
        }

        private static ICollection<ITaskAdapter> GetAddressAdapters(AddressSummaryAdapter summary, IEnumerator<AddressTransactionModel> transactions)
        {
            var result = new Collection<ITaskAdapter>();

            while (transactions.MoveNext())
            {
                var tx = transactions.Current;
                if (tx == null)
                {
                    break;
                }

                if (tx.TxType == TransactionType.CoinBase)
                {
                    summary.Received += tx.Value;
                    summary.Balance += tx.Value;
                }
                else if (tx.TxType == TransactionType.CoinStake)
                {
                    summary.Received += tx.Value;
                    summary.Balance += tx.Value;
                }
                else if (tx.Value > 0)
                {
                    summary.Received += tx.Value;
                    summary.Balance += tx.Value;
                }
                else
                {
                    summary.Sent -= tx.Value;
                    summary.Balance -= tx.Value;
                }

                summary.TxCount++;

                var adapter = new AddressTransactionAdapter(tx);

                if (result.FirstOrDefault(t => t.PartitionKey.Equals(adapter.Address.Partition()) && t.RowKey.Equals(adapter.RowKey)) is AddressTransactionAdapter existingAdapter)
                {
                    existingAdapter.Value += adapter.Value;
                    existingAdapter.TxBalance += adapter.TxBalance;
                }
                else
                {
                    result.Add(adapter);
                }
            }

            return result;
        }

        private async Task<AddressSummaryAdapter> GetAddressSummaryAsync(string address)
        {
            var defaultResult = new AddressSummaryAdapter(new AddressSummaryModel { Address = address });
            var operation = TableOperation.Retrieve<AddressSummaryAdapter>(defaultResult.PartitionKey, defaultResult.RowKey);
            var response = await StorageClient.GetAddressTable().ExecuteAsync(operation).ConfigureAwait(false);
            return response.Result as AddressSummaryAdapter ?? defaultResult;
        }

        private Task<List<AddressTransactionModel>> GetCoinBaseAsync(IBlockInfo blockInfo, Transaction tx)
        {
            var vout = tx.Outputs.Single();
            var address = vout.ScriptPubKey.GetDestinationAddress(_network);
            if (address == null)
            {
                return Task.FromResult(new List<AddressTransactionModel>());
            }

            return Task.FromResult(new List<AddressTransactionModel>
            {
                new AddressTransactionModel
                {
                    Address = address.ToString(),
                    TxId = tx.GetHash(),
                    BlockId = blockInfo.Hash,
                    BlockHeight = blockInfo.Height,
                    Time = NBitcoin.Utils.UnixTimeToDateTime(tx.Time),
                    Value = vout.Value,
                    TxType = TransactionType.CoinBase
                }
            });
        }

        private async Task<List<AddressTransactionModel>> GetCoinStakeAsync(IBlockInfo blockInfo, Transaction tx)
        {
            var prevOut = tx.Inputs.Single().PrevOut;

            var txIn = await _blockRepository.GetTrxAsync(prevOut.Hash).ConfigureAwait(false);
            var txInAddress = txIn.Outputs[prevOut.N].ScriptPubKey.GetDestinationAddress(_network);
            if (txInAddress == null)
            {
                var txInKeys = txIn.Outputs[prevOut.N].ScriptPubKey.GetDestinationPublicKeys();
                txInAddress = txInKeys.Single().GetAddress(_network);
            }

            var txOutAddress1 = tx.Outputs[1].ScriptPubKey.GetDestinationAddress(_network);
            if (txOutAddress1 == null)
            {
                var txOutKeys = tx.Outputs[1].ScriptPubKey.GetDestinationPublicKeys();
                txOutAddress1 = txOutKeys.Single().GetAddress(_network);
            }

            var txInValue = txIn.Outputs[prevOut.N].Value;
            Money change = 0;

            if (tx.Outputs.Count == 2)
            {
                if (!txInAddress.Equals(txOutAddress1))
                {
                    throw new InvalidOperationException("coinstake invalid");
                }

                change = tx.Outputs[1].Value - txInValue;
            }
            else
            {
                var txOutAddress2 = tx.Outputs[2].ScriptPubKey.GetDestinationAddress(_network);
                if (txOutAddress2 == null)
                {
                    var txOutKeys = tx.Outputs[2].ScriptPubKey.GetDestinationPublicKeys();
                    txOutAddress2 = txOutKeys.Single().GetAddress(_network);
                }

                if (!txInAddress.Equals(txOutAddress1) || !txInAddress.Equals(txOutAddress2))
                {
                    throw new InvalidOperationException("coinstake invalid");
                }

                var txOutValue = tx.Outputs[1].Value + tx.Outputs[2].Value;
                change = txOutValue - txInValue;
            }

            return new List<AddressTransactionModel>
            {
                new AddressTransactionModel
                {
                    Kind = AddressKind.Transaction,
                    Address = txInAddress.ToString(),
                    TxId = tx.GetHash(),
                    BlockId = blockInfo.Hash,
                    BlockHeight = blockInfo.Height,
                    Time = NBitcoin.Utils.UnixTimeToDateTime(tx.Time),
                    Value = change,
                    TxType = TransactionType.CoinStake
                }
            };
        }

        private async Task<List<AddressTransactionModel>> GetTransferAsync(IBlockInfo blockInfo, Transaction tx)
        {
            var results = new List<AddressTransactionModel>();

            foreach (var txInput in tx.Inputs)
            {
                var txIn = await _blockRepository.GetTrxAsync(txInput.PrevOut.Hash).ConfigureAwait(false);
                var txInValue = txIn.Outputs[txInput.PrevOut.N].Value;
                var txInAddress = txIn.Outputs[txInput.PrevOut.N].ScriptPubKey.GetDestinationAddress(_network);
                if (txInAddress == null)
                {
                    var txInKeys = txIn.Outputs[txInput.PrevOut.N].ScriptPubKey.GetDestinationPublicKeys();
                    txInAddress = txInKeys.Single().GetAddress(_network);
                }

                results.Add(new AddressTransactionModel
                {
                    Address = txInAddress.ToString(),
                    TxId = tx.GetHash(),
                    BlockId = blockInfo.Hash,
                    BlockHeight = blockInfo.Height,
                    Time = NBitcoin.Utils.UnixTimeToDateTime(tx.Time),
                    Value = -txInValue,
                    TxType = TransactionType.Normal
                });
            };

            foreach (var txOutput in tx.Outputs)
            {
                var txOutAddress = txOutput.ScriptPubKey.GetDestinationAddress(_network);
                if (txOutAddress == null)
                {
                    var txOutKeys = txOutput.ScriptPubKey.GetDestinationPublicKeys();
                    txOutAddress = txOutKeys.Single().GetAddress(_network);
                }

                results.Add(new AddressTransactionModel
                {
                    Address = txOutAddress.ToString(),
                    TxId = tx.GetHash(),
                    BlockId = blockInfo.Hash,
                    BlockHeight = blockInfo.Height,
                    Time = NBitcoin.Utils.UnixTimeToDateTime(tx.Time),
                    Value = txOutput.Value,
                    TxType = TransactionType.Normal
                });
            };

            return results;
        }
    }
}


//protected override async Task<IEnumerable<ITaskAdapter>> ProcessBlocks(IEnumerable<IBlockInfo> blocks)
//{
//    var txModelTasks = GetTransactionModels(blocks);
//    await Task.WhenAll(txModelTasks);

//    var addresses = txModelTasks.SelectMany(t => t.Result)
//        .Select(t => t.Address)
//        .Distinct()
//        .ToList();

//    var txGroups = txModelTasks.SelectMany(t => t.Result)
//        .OrderBy(t => t.BlockHeight)
//        .GroupBy(t => t.Address);

//    var result = new List<ITaskAdapter>();

//    var summaries = await GetAddressSummaries(addresses);

//    foreach (var group in txGroups)
//    {
//        var summary = summaries.FirstOrDefault(s => s.Address.Equals(group.Key));
//        result.AddRange(GetAddressAdapters(summary, group.GetEnumerator()));
//    }

//    result.AddRange(summaries);

//    var duplicates = result.GroupBy(a => new { a.PartitionKey, a.RowKey }).Where(g => g.Count() > 1).ToList();
//    if (duplicates.Any())
//    {

//    }

//    return result;
//}

//protected List<Task<List<AddressTransactionModel>>> GetTransactionModels(IEnumerable<IBlockInfo> blocks)
//{
//    var transactions = new List<Task<List<AddressTransactionModel>>>();
//    foreach (var block in blocks)
//    {
//        foreach (var tx in block.Block.Transactions)
//        {
//            if (tx.IsCoinBase)
//            {
//                transactions.Add(GetCoinBaseAsync(block, tx));
//            }
//            else if (tx.IsCoinStake)
//            {
//                transactions.Add(GetCoinStakeAsync(block, tx));
//            }
//            else
//            {
//                transactions.Add(GetTransferAsync(block, tx));
//            }
//        }
//    }
//    return transactions;
//}

//private async Task<ICollection<AddressSummaryAdapter>> GetAddressSummaries(ICollection<string> addresses)
//{
//    var taskCount = 0;
//    var taskThreshold = 100;

//    var summaryTasks = new List<Task<AddressSummaryAdapter>>(addresses.Count);
//    foreach (var address in addresses)
//    {
//        taskCount++;
//        summaryTasks.Add(GetAddressSummary(address));
//        if (taskCount < taskThreshold)
//        {
//            continue;
//        }
//        await Task.WhenAll(summaryTasks);
//        taskCount = 0;
//    }

//    await Task.WhenAll(summaryTasks);

//    return summaryTasks.Select(t => t.Result).ToList();
//}

//private async Task LoadAddresses1(IEnumerable<BitcoinAddress> addresses)
//{
//    var table = StorageClient.GetAddressTable();
//    var tasks = (from address in addresses select new AddressSummaryAdapter(new AddressSummaryModel {Kind = AddressKind.Summary, Address = address.ToString()}) 
//        into defaultResult select TableOperation.Retrieve<AddressSummaryAdapter>(defaultResult.PartitionKey, defaultResult.RowKey) 
//        into operation
//        select table.ExecuteAsync(operation)).ToList();

//    await Task.WhenAll(tasks);

//    var adapters = tasks.Select(t => t.Result.Result).OfType<AddressSummaryAdapter>().ToList();

//    foreach (var address in addresses)
//    {
//        var summary = adapters.FirstOrDefault(a => a.PartitionKey.Equals(address.ToString()));
//        var result = summary ?? new AddressSummaryAdapter(new AddressSummaryModel { Kind = AddressKind.Summary, Address = address.ToString() });
//        _summaryAdapters.AddOrUpdate(result.PartitionKey, result, (key, adapter) => adapter);
//    }
//}

//private async Task<IEnumerable<BitcoinAddress>> GetInputAddresses(IEnumerable<IBlockInfo> blocks)
//{
//    var results = new HashSet<BitcoinAddress>();

//    var vinTasks = blocks
//        .SelectMany(b => b.Block.Transactions)
//        .SelectMany(tx => tx.Inputs)
//        .Select(i => _blockRepository.GetTrxAsync(i.PrevOut.Hash))
//        .ToList();

//    await Task.WhenAll(vinTasks);

//    var inputs = vinTasks.Select(t => t.Result).OfType<Transaction>();

//    Parallel.ForEach(inputs, txIn =>
//    {
//        var address1 = txIn.Outputs[vin.PrevOut.N].ScriptPubKey.GetDestinationAddress(_network);
//        if (address1 != null)
//        {
//            results.Add(address1);
//        }

//        var keys = txIn.Outputs[vin.PrevOut.N].ScriptPubKey.GetDestinationPublicKeys();
//        var addresses = keys.Select(k => k.ScriptPubKey.GetDestinationAddress(_network));
//        foreach (var address2 in addresses)
//        {
//            if (address2 != null)
//            {
//                results.Add(address2);
//            }
//        }
//    });

//    var outputs = blocks
//        .SelectMany(b => b.Block.Transactions)
//        .SelectMany(tx => tx.Outputs)
//        .ToList();

//    Parallel.ForEach(outputs, vout =>
//    {
//        var address1 = vout.ScriptPubKey.GetDestinationAddress(_network);
//        if (address1 != null)
//        {
//            results.Add(address1);
//        }

//        var keys = vout.ScriptPubKey.GetDestinationPublicKeys();
//        var addresses = keys.Select(k => k.ScriptPubKey.GetDestinationAddress(_network));
//        foreach (var address2 in addresses)
//        {
//            if (address2 != null)
//            {
//                results.Add(address2);
//            }
//        }
//    });

//    return results;
//}

//private HashSet<BitcoinAddress> GetOutputAddresses(IEnumerable<IBlockInfo> blocks)
//{
//    var results = new HashSet<BitcoinAddress>();



//    return results;
//}


//private async Task ProcessCoinBaseAsync(IBlockInfo blockInfo, Transaction tx)
//{
//    var vout = tx.Outputs.Single();
//    var address = vout.ScriptPubKey.GetDestinationAddress(_network);
//    if (address == null)
//    {
//        return;
//    }

//    var summary = await GetAddressSummary(address);
//    summary.ETag = "*";
//    summary.Balance += vout.Value;
//    summary.Received += vout.Value;
//    summary.TxCount++;

//    AddTransaction(blockInfo, tx, address, vout.Value, summary.Balance);
//}

//private async Task ProcessCoinStakeAsync(IBlockInfo blockInfo, Transaction tx)
//{
//    var prevOut = tx.Inputs.Single().PrevOut;

//    var txIn = await _blockRepository.GetTrxAsync(prevOut.Hash);
//    var txInAddress = txIn.Outputs[prevOut.N].ScriptPubKey.GetDestinationAddress(_network);
//    if (txInAddress == null)
//    {
//        var txInKeys = txIn.Outputs[prevOut.N].ScriptPubKey.GetDestinationPublicKeys();
//        txInAddress = txInKeys.Single().GetAddress(_network);
//    }

//    var txOutAddress1 = tx.Outputs[1].ScriptPubKey.GetDestinationAddress(_network);
//    if (txOutAddress1 == null)
//    {
//        var txOutKeys = tx.Outputs[1].ScriptPubKey.GetDestinationPublicKeys();
//        txOutAddress1 = txOutKeys.Single().GetAddress(_network);
//    }

//    var txOutAddress2 = tx.Outputs[2].ScriptPubKey.GetDestinationAddress(_network);
//    if (txOutAddress2 == null)
//    {
//        var txOutKeys = tx.Outputs[2].ScriptPubKey.GetDestinationPublicKeys();
//        txOutAddress2 = txOutKeys.Single().GetAddress(_network);
//    }

//    if (!txInAddress.Equals(txOutAddress1) || !txInAddress.Equals(txOutAddress2))
//    {
//        throw new InvalidOperationException("coinstake invalid");
//    }

//    var txInValue = txIn.Outputs[prevOut.N].Value;
//    var txOutValue = tx.Outputs[1].Value + tx.Outputs[2].Value;
//    var change = txOutValue - txInValue;

//    var summary = await GetAddressSummary(txInAddress);
//    summary.ETag = "*";
//    summary.Balance += change;
//    summary.Staked += change;
//    summary.TxCount++;

//    AddTransaction(blockInfo, tx, txInAddress, change, summary.Balance);
//}

//private async Task ProcessTransferAsync(IBlockInfo blockInfo, Transaction tx)
//{
//    foreach (var txInput in tx.Inputs)
//    {
//        var txIn = _blockRepository.GetTrxAsync(txInput.PrevOut.Hash).GetAwaiter().GetResult();
//        var txInValue = txIn.Outputs[txInput.PrevOut.N].Value;
//        var txInAddress = txIn.Outputs[txInput.PrevOut.N].ScriptPubKey.GetDestinationAddress(_network);
//        if (txInAddress == null)
//        {
//            var txInKeys = txIn.Outputs[txInput.PrevOut.N].ScriptPubKey.GetDestinationPublicKeys();
//            txInAddress = txInKeys.Single().GetAddress(_network);
//        }

//        var summary = await GetAddressSummary(txInAddress);
//        summary.Balance -= txInValue;
//        summary.Sent += txInValue;
//        summary.TxCount++;

//        AddTransaction(blockInfo, tx, txInAddress, txInValue, summary.Balance);
//    };

//    foreach (var txOutput in tx.Outputs)
//    {
//        var txOutAddress = txOutput.ScriptPubKey.GetDestinationAddress(_network);
//        if (txOutAddress == null)
//        {
//            var txOutKeys = txOutput.ScriptPubKey.GetDestinationPublicKeys();
//            txOutAddress = txOutKeys.Single().GetAddress(_network);
//        }

//        var summary = await GetAddressSummary(txOutAddress);
//        summary.Balance += txOutput.Value;
//        summary.Received += txOutput.Value;
//        summary.TxCount++;

//        AddTransaction(blockInfo, tx, txOutAddress, txOutput.Value, summary.Balance);
//    };
//}
