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
        public int Height
        {
            get;
            set;
        }
        public uint256 BlockId
        {
            get;
            set;
        }
        public Block Block
        {
            get;
            set;
        }
    }
    public class BlockFetcher : IEnumerable<BlockInfo>
    {

        private readonly Checkpoint _checkpoint;
        public Checkpoint Checkpoint
        {
            get
            {
                return _checkpoint;
            }
        }

        private readonly IBlocksRepository _blocksRepository;
        public IBlocksRepository BlocksRepository
        {
            get
            {
                return _blocksRepository;
            }
        }

        private readonly ChainBase _blockHeaders;
        public ChainBase BlockHeaders
        {
            get
            {
                return _blockHeaders;
            }
        }

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

            _blockHeaders = chain;
            _blocksRepository = blocksRepository;
            _checkpoint = checkpoint;
            LastProcessed = lastProcessed;

            InitDefault();
        }

        public TimeSpan NeedSaveInterval
        {
            get;
            set;
        }

        public CancellationToken CancellationToken
        {
            get;
            set;
        }

        #region IEnumerable<BlockInfo> Members

        public ChainedBlock LastProcessed { get; private set; }

        public IEnumerator<BlockInfo> GetEnumerator()
        {
            var lastLogs = new Queue<DateTime>();
            var lastHeights = new Queue<int>();

            var fork = _blockHeaders.FindFork(_checkpoint.BlockLocator);
            var headers = _blockHeaders.EnumerateAfter(fork);
            headers = headers.Where(h => h.Height <= ToHeight);
            var first = headers.FirstOrDefault();
            if(first == null)
                yield break;
            var height = first.Height;
            if(first.Height == 1)
            {
                headers = new[] { fork }.Concat(headers);
                height = 0;
            }

            foreach(var block in _blocksRepository.GetBlocks(headers.Select(b => b.HashBlock), CancellationToken))
            {
                var header = _blockHeaders.GetBlock(height);

                if (block == null)
                {
                    var storeTip = _blocksRepository.GetStoreTip();
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

                IndexerTrace.Processed(height, Math.Min(ToHeight, _blockHeaders.Tip.Height), lastLogs, lastHeights);
                height++;
            }
        }

        internal void SkipToEnd()
        {
            var height = Math.Min(ToHeight, _blockHeaders.Tip.Height);
            LastProcessed = _blockHeaders.GetBlock(height);
            IndexerTrace.Information($"Skipped to the end at height {height}");
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private DateTime _lastSaved = DateTime.UtcNow;
        public bool NeedSave
        {
            get
            {
                return (DateTime.UtcNow - _lastSaved) > NeedSaveInterval;
            }
        }

        public void SaveCheckpoint()
        {
            if(LastProcessed != null)
            {
                _checkpoint.SaveProgress(LastProcessed);
                IndexerTrace.CheckpointSaved(LastProcessed, _checkpoint.CheckpointName);
            }
            _lastSaved = DateTime.UtcNow;
        }

        public int FromHeight
        {
            get;
            set;
        }

        public int ToHeight
        {
            get;
            set;
        }
    }
}
