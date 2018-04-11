using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public sealed class TransactionIndexer : AzureTaskIndexer<TransactionTask>
    {
        public override CheckpointType CheckPointType { get; } = CheckpointType.Transaction;

        public TransactionIndexer(
            TransactionTaskFactory taskFactory,
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