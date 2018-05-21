using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Extensions;
using Zorbit.Features.Observatory.Core.Model;

namespace Zorbit.Features.Observatory.Core.Node
{
    public interface INodeBlockFetcher
    {
        int ToHeight { get; set; }
        int FromHeight { get; set; }
        ChainedBlock LastProcessed { get; }
        CancellationToken CancellationToken { get; set; }
        IEnumerable<IBlockInfo> GetBlocks();
    }

    public class NodeBlockFetcher : INodeBlockFetcher
    {
        private readonly INodeBlockStore _nodeBlocks;

        private readonly ConcurrentChain _chain;

        private readonly ILogger _logger;

        public NodeBlockFetcher(
            INodeBlockStore nodeBlocks,
            ConcurrentChain chain,
            ChainedBlock lastProcessed,
            ILoggerFactory loggerFactory)
        {
            _nodeBlocks = nodeBlocks ?? throw new ArgumentNullException(nameof(nodeBlocks));
            _chain = chain ?? throw new ArgumentNullException(nameof(chain));
            _logger = loggerFactory.CreateLogger<NodeBlockFetcher>();
            ToHeight = int.MaxValue;
            LastProcessed = lastProcessed ?? throw new ArgumentNullException(nameof(lastProcessed));
        }

        public IEnumerable<IBlockInfo> GetBlocks()
        {
            var fork = _chain.FindFork(LastProcessed.GetLocator());
            var headers = _chain
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

            foreach (var block in _nodeBlocks.GetBlocks(headers.Select(_ => _.HashBlock), CancellationToken))
            {
                var header = _chain.GetBlock(height);

                if (block == null)
                {
                    var storeTip = _nodeBlocks.GetStoreTip();
                    if (storeTip != null)
                    {
                        // Store is caught up with Chain but the block is missing from the store.
                        if (header.Header.BlockTime <= storeTip.Header.BlockTime)
                        {
                            throw new InvalidOperationException($"Chained block not found in store (height = { height }). Re-create the block store.");
                        }
                    }

                    // Allow Store to catch up with Chain.
                    break;
                }

                LastProcessed = header;
                yield return new BlockInfoModel()
                {
                    Block = block,
                    Hash = header.HashBlock,
                    Height = header.Height
                };
                height++;
            }
        }

        public CancellationToken CancellationToken { get; set; }

        public ChainedBlock LastProcessed { get; private set; }

        public int FromHeight { get; set; }

        public int ToHeight { get; set; }
    }
}
