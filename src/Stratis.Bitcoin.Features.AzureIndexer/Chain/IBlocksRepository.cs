using System.Collections.Generic;
using System.Threading;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public interface IBlocksRepository
    {
        Block GetStoreTip();

        IEnumerable<Block> GetBlocks(IEnumerable<uint256> hashes, CancellationToken cancellationToken);
    }

    public class FullNodeBlocksRepository : IBlocksRepository
    {
        private readonly FullNode _node;
        private readonly BlockStore.BlockRepository _repo;

        public FullNodeBlocksRepository(FullNode node)
        {
            _node = node;
            _repo = node.NodeService<BlockStore.IBlockRepository>() as BlockStore.BlockRepository;
        }

        public Block GetStoreTip()
        {
            return _repo.GetAsync(_repo.BlockHash).GetAwaiter().GetResult();
        }

        public IEnumerable<Block> GetBlocks(IEnumerable<uint256> hashes, CancellationToken cancellationToken)
        {
            foreach (var hash in hashes)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (hash == _node.Network.GenesisHash)
                    yield return _node.Network.GetGenesis();
                else
                    yield return _repo.GetAsync(hash).GetAwaiter().GetResult();
            }
        }
    }
}
