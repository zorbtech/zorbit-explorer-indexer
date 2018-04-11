using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public sealed class BlockSummaryIndexer : AzureTaskIndexer<BlockSummaryTask>
    {
        public override CheckpointType CheckPointType { get; } = CheckpointType.Summary;

        public BlockSummaryIndexer(
            BlockSummaryTaskFactory taskFactory,
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
