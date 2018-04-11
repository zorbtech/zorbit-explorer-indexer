using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public enum CheckpointType
    {
        Block,
        Summary,
        Transaction,
        Balance,
        Wallet,
        Chain
    }

    public interface IAzureIndexer
    {
        AzureStorageClient StorageClient { get; }
        TaskScheduler TaskScheduler { get; set; }
        ChainedBlock Tip { get; }
        CheckpointType CheckPointType { get; }
        CheckpointRepository GetCheckpointRepository();
        Task IndexAsync(CancellationToken cancellationToken);
        Task Initialize(CancellationToken cancellationToken);
    }

    public abstract class AbstractAzureIndexer : IAzureIndexer
    {
        protected AbstractAzureIndexer(
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            this.FullNode = fullNode;
            this.Chain = chain;
            this.StorageClient = storageClient ?? throw new ArgumentNullException("storageClient");
            this.Settings = settings;
        }

        public abstract Task IndexAsync(CancellationToken cancellationToken);

        public abstract CheckpointType CheckPointType { get; }

        public virtual Task Initialize(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public CheckpointRepository GetCheckpointRepository()
        {
            var checkPointName = string.IsNullOrWhiteSpace(this.Settings.CheckpointsetName) ?
                "default" :
                this.Settings.CheckpointsetName;
            return new CheckpointRepository(this.StorageClient.GetBlocksContainer(), this.StorageClient.Network, checkPointName);
        }

        protected Task IndexAsync(IEnumerable<ITableEntity> entities, CloudTable table)
        {
            var task = new IndexTableEntitiesTask(this.StorageClient, table);
            return task.IndexAsync(entities, this.TaskScheduler);
        }

        protected void SetThrottling()
        {
            Helper.SetThrottling();
            var tableServicePoint = ServicePointManager.FindServicePoint(this.StorageClient.TableClient.BaseUri);
            tableServicePoint.ConnectionLimit = 1000;
        }

        /// <summary>
        /// Determines the block that a checkpoint is at.
        /// </summary>
        /// <param name="checkpointType">The type of checkpoint (wallets, blocks, transactions or balances).</param>
        /// <returns>The block that a checkpoint is at.</returns>
        protected async Task<ChainedBlock> GetCheckPointBlockAsync(CheckpointType checkpointType)
        {
            var checkpoint = await GetCheckPoint(checkpointType);
            return this.Chain.FindFork(checkpoint.BlockLocator);
        }

        /// <summary>
        /// Gets a block fetcher that respects the given type of checkpoint.
        /// The block fetcher will return "BatchSize" blocks starting at this.StoreTip + 1.
        /// If "this._chainIndexer.IgnoreCheckpoints" is set then the checkpoints 
        /// will be ignored by "GetCheckpointInternal".
        /// </summary>
        /// <param name="checkpointType">The type of checkpoint (wallets, blocks, transactions or balances).</param>
        /// <param name="cancellationToken">The token used for cancellation.</param>
        /// <returns>A block fetcher that respects the given type of checkpoint.</returns>
        protected async Task<BlockFetcher> GetBlockFetcherAsync(CheckpointType checkpointType, ChainedBlock lastProcessed, CancellationToken cancellationToken)
        {
            var checkpoint = await GetCheckPoint(checkpointType);
            var repo = new FullNodeBlocksRepository(this.FullNode);
            return new BlockFetcher(checkpoint, repo, this.Chain, lastProcessed)
            {
                NeedSaveInterval = this.Settings.CheckpointInterval,
                FromHeight = this.Tip.Height + 1,
                ToHeight = Math.Min(this.Tip.Height + this.Settings.BatchSize, this.Settings.To),
                CancellationToken = cancellationToken
            };
        }

        private async Task<Checkpoint> GetCheckPoint(CheckpointType checkpointType)
        {
            var checkpointName = checkpointType.ToString().ToLowerInvariant();
            var checkpoint = await this.GetCheckpointRepository().GetCheckpointAsync(checkpointName);
            if (this.Settings.IgnoreCheckpoints)
            {
                checkpoint = new Checkpoint(checkpoint.CheckpointName, this.StorageClient.Network, null, null);
            }

            return checkpoint;
        }

        protected FullNode FullNode { get; }

        protected BlockFetcher BlockFetcher { get; set; }

        protected ConcurrentChain Chain { get; }

        protected AzureIndexerSettings Settings { get; }

        public AzureStorageClient StorageClient { get; }

        public ChainedBlock Tip { get; protected set; }

        public TaskScheduler TaskScheduler { get; set; } = TaskScheduler.Default;
    }
}