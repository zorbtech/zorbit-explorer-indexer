using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Extensions;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IBlockInfo
    {
        uint256 Hash { get; set; }
        int Height { get; set; }
        Block Block { get; set; }
        IEnumerable<IBlockChunk> GetChunks();
        void ParseChunks(IEnumerable<IBlockChunk> chunks);
    }

    public sealed class BlockInfoModel : IBlockInfo
    {
        public uint256 Hash { get; set; }
        public int Height { get; set; }
        public Block Block { get; set; }

        public BlockInfoModel()
        {
        }

        public BlockInfoModel(IBlockInfo blockInfo)
        {
            Hash = blockInfo.Hash;
            Height = blockInfo.Height;
            Block = blockInfo.Block;
        }

        public IEnumerable<IBlockChunk> GetChunks()
        {
            var results = new List<IBlockChunk>();

            var block = Block.ToBytes();
            var parts = 0;
            var index = 0;

            var b = new BlockChunkModel()
            {
                Hash = Hash,
                Index = index
            };

            results.Add(b);

            foreach (var part in block.Split(64))
            {
                b.Chunks.Add(part.ToArray());
                parts++;

                if (parts != 200)
                {
                    continue;
                }

                parts = 0;
                index++;
                b = new BlockChunkModel
                {
                    Hash = Hash,
                    Index = index
                };
                results.Add(b);
            }

            return results;
        }

        public void ParseChunks(IEnumerable<IBlockChunk> chunks)
        {
            var chunks2 = chunks.ToList();
            if (!chunks2.Any())
            {
                return;
            }

            IEnumerable<byte> bytes = new List<byte>();
            bytes = chunks2.SelectMany(entry => entry.Chunks)
                .Aggregate(bytes, (current, chunk) => current.Concat(chunk));

            Block = new Block(bytes.ToArray());
        }
    }
}
