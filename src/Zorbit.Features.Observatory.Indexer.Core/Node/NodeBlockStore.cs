using System.Collections.Generic;
using System.Threading;
using NBitcoin;
using Stratis.Bitcoin;
using Stratis.Bitcoin.Features.BlockStore;

namespace Zorbit.Features.Observatory.Core.Node
{
    public interface INodeBlockStore
    {
        IEnumerable<Block> GetBlocks(IEnumerable<uint256> hashes, CancellationToken cancellationToken);
        Block GetStoreTip();
    }

    public class NodeBlockStore : INodeBlockStore
    {
        private readonly FullNode _fullNode;
        private readonly BlockRepository _repo;

        public NodeBlockStore(FullNode fullNode)
        {
            _fullNode = fullNode;
            _repo = fullNode.NodeService<IBlockRepository>() as BlockRepository;
        }

        public Block GetStoreTip()
        {
            return _repo.GetAsync(_repo.BlockHash).GetAwaiter().GetResult();
        }

        public IEnumerable<Block> GetBlocks(IEnumerable<uint256> hashes, CancellationToken cancellationToken)
        {
            foreach (var hash in hashes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (hash == _fullNode.Network.GenesisHash)
                {
                    yield return _fullNode.Network.GetGenesis();
                }
                else
                {
                    yield return _repo.GetAsync(hash).GetAwaiter().GetResult();
                }
            }
        }
    }
}
