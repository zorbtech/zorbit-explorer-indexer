using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public abstract class AzureTaskIndexer<T> : AbstractAzureIndexer
        where T : IIndexTask
    {
        private readonly IIndexTaskFactory<T> _indexTaskFactory;

        protected ILogger Logger { get; }

        protected AzureTaskIndexer(
            IIndexTaskFactory<T> indexTaskFactory,
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings,
            ILoggerFactory loggerFactor)
            : base(fullNode, chain, storageClient, settings)
        {
            _indexTaskFactory = indexTaskFactory;
            Logger = loggerFactor.CreateLogger<BalanceIndexer>();
        }
        
        public override async Task Initialize(CancellationToken cancellationToken)
        {
            var checkpoint = await GetCheckPointBlockAsync(CheckPointType);
            var height = Settings.IgnoreCheckpoints ? Settings.From : checkpoint.Height;
            Tip = Chain.GetBlock(height);
            BlockFetcher = await GetBlockFetcherAsync(CheckPointType, checkpoint, cancellationToken);
        }

        public override async Task IndexAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("()");

            while (Tip.Height < Settings.To && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (Chain.Height == 0)
                    {
                        await Task.Delay(5000, cancellationToken);
                        continue;
                    }

                    BlockFetcher.FromHeight = Math.Max(BlockFetcher.LastProcessed.Height + 1, Tip.Height + 1);
                    BlockFetcher.ToHeight = Math.Min(Tip.Height + Settings.BatchSize, Settings.To);

                    _indexTaskFactory.CreateTask().Index(BlockFetcher, TaskScheduler);

                    Tip = Chain.GetBlock(BlockFetcher.LastProcessed.Height);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);

                    IndexerTrace.ErrorWhileImportingBlockToAzure(Tip.HashBlock, ex);

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
            }

            Logger.LogTrace("(-)");
        }
    }
}