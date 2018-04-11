using System.Collections.Generic;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Balance;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Wallet;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public class BalanceTask : IndexTableEntitiesTaskBase<OrderedBalanceChange>
    {
        private readonly string _partitionKey;
        private readonly WalletRuleEntryCollection _walletRules;

        public BalanceTask(
            string partitionKey,
            AzureStorageClient storageClient, 
            WalletRuleEntryCollection walletRules)
            : base(storageClient)
        {
            _partitionKey = partitionKey;
            _walletRules = walletRules;
        }

        protected override Microsoft.WindowsAzure.Storage.Table.CloudTable GetCloudTable()
        {
            return StorageClient.GetBalanceTable();
        }

        protected override Microsoft.WindowsAzure.Storage.Table.ITableEntity ToTableEntity(OrderedBalanceChange item)
        {
            return item.ToEntity();
        }

        protected override void ProcessBlock(BlockInfo blockInfo, BulkImport<OrderedBalanceChange> bulk)
        {
            foreach (var tx in blockInfo.Block.Transactions)
            {
                var txId = tx.GetHash();

                var entries = Extract(txId, tx, blockInfo.BlockId, blockInfo.Block.Header, blockInfo.Height);
                foreach (var entry in entries)
                {
                    bulk.Add(entry.PartitionKey, entry);
                }
            }
        }

        private IEnumerable<OrderedBalanceChange> Extract(uint256 txId, Transaction tx, uint256 blockId, BlockHeader blockHeader, int height)
        {
            return _walletRules != null ?
                OrderedBalanceChange.ExtractWalletBalances(txId, tx, blockId, blockHeader, height, _walletRules) :
                OrderedBalanceChange.ExtractScriptBalances(txId, tx, blockId, blockHeader, height);
        }

        protected override bool SkipToEnd => _walletRules != null && _walletRules.Count == 0;
    }
}

