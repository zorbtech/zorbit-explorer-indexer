using System.Collections.Generic;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.ReadModel
{
    public class Tx
    {
        public uint256 hash { get; set; }
        public int ver { get; set; }
        public int vin_sz { get; set; }
        public int vout_sz { get; set; }
        public int lock_time { get; set; }
        public int size { get; set; }
        public List<In> @in { get; set; }
        public List<PrevOut> @out { get; set; }
    }
}