using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public sealed class BalanceIndexer : AzureTaskIndexer<BalanceTask>
    {
        public override CheckpointType CheckPointType { get; } = CheckpointType.Balance;

        public BalanceIndexer(
            BalanceTaskFactory taskFactory,
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings,
            ILoggerFactory loggerFactory)
            : base(taskFactory, fullNode, chain, storageClient, settings, loggerFactory)
        {
        }
    }
}
