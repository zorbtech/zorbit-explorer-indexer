using System.Collections.Generic;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IBlockChunk
    {
        uint256 Hash { get; set; }
        int Index { get; set; }
        int Height { get; set; }
        IList<byte[]> Chunks { get; set; }
    }

    public class BlockChunkModel : IBlockChunk
    {
        public uint256 Hash { get; set; }
        public int Index { get; set; }
        public int Height { get; set; }
        public IList<byte[]> Chunks { get; set; } = new List<byte[]>();
    }
}