using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class BlockInfo
    {
        public int Height { get; set; }

        public uint256 BlockId { get; set; }

        public Block Block { get; set; }
    }

    public class BlockFetcher : IEnumerable<BlockInfo>
    {
        private DateTime _lastSaved = DateTime.UtcNow;

        private void InitDefault()
        {
            NeedSaveInterval = TimeSpan.FromMinutes(15);
            ToHeight = int.MaxValue;
        }

        public BlockFetcher(Checkpoint checkpoint, IBlocksRepository blocksRepository, ChainBase chain, ChainedBlock lastProcessed)
        {
            if (blocksRepository == null)
                throw new ArgumentNullException("blocksRepository");

            if (chain == null)
                throw new ArgumentNullException("blockHeaders");

            if (checkpoint == null)
                throw new ArgumentNullException("checkpoint");

            BlockHeaders = chain;
            BlocksRepository = blocksRepository;
            Checkpoint = checkpoint;
            LastProcessed = lastProcessed;

            InitDefault();
        }

        public IEnumerator<BlockInfo> GetEnumerator()
        {
            var lastLogs = new Queue<DateTime>();
            var lastHeights = new Queue<int>();

            var fork = BlockHeaders.FindFork(Checkpoint.BlockLocator);
            var headers = BlockHeaders
                .EnumerateAfter(fork).Where(h => h.Height <= ToHeight)
                .ToList();

            var first = headers.FirstOrDefault();
            if (first == null)
            {
                yield break;
            }

            var height = first.Height;
            if (first.Height == 1)
            {
                var headersWithGenesis = new List<ChainedBlock> { fork };
                headers = headersWithGenesis.Concat(headers).ToList();
                height = 0;
            }

            foreach (var block in BlocksRepository.GetBlocks(headers.Select(b => b.HashBlock), CancellationToken))
            {
                var header = BlockHeaders.GetBlock(height);

                if (block == null)
                {
                    var storeTip = BlocksRepository.GetStoreTip();
                    if (storeTip != null)
                    {
                        // Store is caught up with Chain but the block is missing from the store.
                        if (header.Header.BlockTime <= storeTip.Header.BlockTime)
                            throw new InvalidOperationException($"Chained block not found in store (height = { height }). Re-create the block store.");
                    }
                    // Allow Store to catch up with Chain.
                    break;
                }

                LastProcessed = header;
                yield return new BlockInfo()
                {
                    Block = block,
                    BlockId = header.HashBlock,
                    Height = header.Height
                };

                IndexerTrace.Processed(height, Math.Min(ToHeight, BlockHeaders.Tip.Height), lastLogs, lastHeights);
                height++;
            }
        }

        public void SaveCheckpoint()
        {
            if (LastProcessed != null)
            {
                Checkpoint.SaveProgress(LastProcessed);
                IndexerTrace.CheckpointSaved(LastProcessed, Checkpoint.CheckpointName);
            }
            _lastSaved = DateTime.UtcNow;
        }

        internal void SkipToEnd()
        {
            var height = Math.Min(ToHeight, BlockHeaders.Tip.Height);
            LastProcessed = BlockHeaders.GetBlock(height);
            IndexerTrace.Information($"Skipped to the end at height {height}");
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CancellationToken CancellationToken { get; set; }

        public TimeSpan NeedSaveInterval { get; set; }

        public ChainedBlock LastProcessed { get; private set; }

        public int FromHeight { get; set; }

        public int ToHeight { get; set; }

        public Checkpoint Checkpoint { get; }

        public IBlocksRepository BlocksRepository { get; }

        public ChainBase BlockHeaders { get; }

        public bool NeedSave => (DateTime.UtcNow - _lastSaved) > NeedSaveInterval;
    }
}
