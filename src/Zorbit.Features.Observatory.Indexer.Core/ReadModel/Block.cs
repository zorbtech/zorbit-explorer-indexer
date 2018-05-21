using System.Collections.Generic;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.ReadModel
{
    public class Block
    {
        public uint256 hash { get; set; }
        public int ver { get; set; }
        public string prev_block { get; set; }
        public string mrkl_root { get; set; }
        public int time { get; set; }
        public int bits { get; set; }
        public long nonce { get; set; }
        public int n_tx { get; set; }
        public int size { get; set; }
        public List<Tx> tx { get; set; }
    }
}