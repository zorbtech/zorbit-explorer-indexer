using Microsoft.Extensions.Logging;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Node
{
    public interface INodeBlockFetcherFactory
    {
        INodeBlockFetcher Create(ChainedBlock tip);
    }

    public sealed class NodeBlockFetcherFactory : INodeBlockFetcherFactory
    {
        private readonly INodeBlockStore _nodeBlocks;

        private readonly ConcurrentChain _chain;

        private readonly ILoggerFactory _loggerFactory;

        public NodeBlockFetcherFactory(
            INodeBlockStore nodeBlocks,
            ConcurrentChain chain,
            ILoggerFactory loggerFactory)
        {
            _nodeBlocks = nodeBlocks;
            _chain = chain;
            _loggerFactory = loggerFactory;
        }

        public INodeBlockFetcher Create(ChainedBlock tip)
        {
            return new NodeBlockFetcher(_nodeBlocks, _chain, tip, _loggerFactory);
        }
    }
}