namespace Zorbit.Features.Observatory.Core.ReadModel
{
    public class In
    {
        public PrevOut prev_out { get; set; }
        public string coinbase { get; set; }
        public int sequence { get; set; }
        public string scriptSig { get; set; }
    }
}