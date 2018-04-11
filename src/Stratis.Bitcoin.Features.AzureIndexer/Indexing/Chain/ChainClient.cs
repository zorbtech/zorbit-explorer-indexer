using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Chain
{
    public interface IChainClient
    {
        IEnumerable<ChainBlockHeader> GetChainChangesUntilFork(ChainedBlock currentTip, bool forkIncluded, CancellationToken cancellation = default(CancellationToken));

        ConcurrentChain GetMainChain();
    }

    public sealed class ChainClient : IChainClient
    {
        private readonly AzureStorageClient _storageClient;

        public ChainClient(
            AzureStorageClient storageClient)
        {
            _storageClient = storageClient;
        }

        public ConcurrentChain GetMainChain()
        {
            var chain = new ConcurrentChain();
            SynchronizeChain(chain);
            return chain;
        }

        public IEnumerable<ChainBlockHeader> GetChainChangesUntilFork(ChainedBlock currentTip, bool forkIncluded, CancellationToken cancellation = default(CancellationToken))
        {
            var oldTip = currentTip;
            var table = _storageClient.GetChainTable();
            var blocks = new List<ChainBlockHeader>();

            var incl1 = table.ExecuteQuery(new TableQuery()).Skip(2).Select(e => new ChainPartEntity(e)).ToList();
            //var incl2 = table.ExecuteQuery(new TableQuery()).Skip(12).Select(e => new ChainPartEntry(e)).ToList();

            var balance = ExecuteBalanceQuery(table, new TableQuery(), new[] {1, 2, 10})
                .Select(e => new ChainPartEntity(e));

            var history = balance.Concat(incl1).ToList();

            foreach (var chainPart in history)
            {
                cancellation.ThrowIfCancellationRequested();

                var height = chainPart.Height;

                if (currentTip == null && oldTip != null)
                {
                    throw new InvalidOperationException("No fork found, the chain stored in azure is probably different from the one of the provided input");
                }

                if (oldTip == null || height > currentTip.Height)
                {
                    yield return CreateChainChange(height, chainPart.BlockHeader);
                }
                else
                {
                    if (height < currentTip.Height)
                    {
                        currentTip = currentTip.GetAncestor(height);
                    }

                    if (currentTip == null || height > currentTip.Height)
                    {
                        throw new InvalidOperationException("Ancestor block not found in chain.");
                    }

                    var chainChange = CreateChainChange(height, chainPart.BlockHeader);
                    if (chainChange.BlockId == currentTip.HashBlock)
                    {
                        if (forkIncluded)
                        {
                            yield return chainChange;
                        }

                        yield break;
                    }

                    yield return chainChange;
                    currentTip = currentTip.Previous;
                }
                height--;
            }
        }

        private static ChainBlockHeader CreateChainChange(int height, BlockHeader block)
        {
            return new ChainBlockHeader()
            {
                Height = height,
                Header = block,
                BlockId = block.GetHash()
            };
        }

        private static IEnumerable<DynamicTableEntity> ExecuteBalanceQuery(CloudTable table, TableQuery tableQuery, IEnumerable<int> pages)
        {
            pages = pages ?? new int[0];
            var pagesEnumerator = pages.GetEnumerator();
            TableContinuationToken continuation = null;
            do
            {
                tableQuery.TakeCount = pagesEnumerator.MoveNext() ? (int?)pagesEnumerator.Current : null;

                var segment = table.ExecuteQuerySegmentedAsync(tableQuery, continuation).GetAwaiter().GetResult();
                continuation = segment.ContinuationToken;

                foreach (var entity in segment)
                {
                    yield return entity;
                }

            } while (continuation != null);
        }

        private void SynchronizeChain(ChainBase chain)
        {
            if (chain.Tip != null && chain.Genesis.HashBlock != _storageClient.Network.GetGenesis().GetHash())
            {
                throw new ArgumentException("Incompatible Network between the indexer and the chain", "chain");
            }

            if (chain.Tip == null)
            {
                var genesis = _storageClient.Network.GetGenesis();
                chain.SetTip(new ChainedBlock(genesis.Header, genesis.GetHash(), 0));
            }

            GetChainChangesUntilFork(chain.Tip, false).UpdateChain(chain);
        }
    }
}
