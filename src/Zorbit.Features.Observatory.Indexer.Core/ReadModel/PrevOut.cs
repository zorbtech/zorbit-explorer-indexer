using NBitcoin;

namespace Zorbit.Features.Observatory.Core.ReadModel
{
    public class PrevOut
    {
        public uint256 hash { get; set; }
        public long n { get; set; }
    }
}
