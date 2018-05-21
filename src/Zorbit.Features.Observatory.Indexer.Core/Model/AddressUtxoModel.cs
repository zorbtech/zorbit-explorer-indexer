using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IAddressUtxo : IAddress
    {
        int BlockHeight { get; set; }
        int TxIndex { get; set; }
        Money Value { get; set; }
        uint256 TxSpentId { get; set; }
        int TxSpentHeight { get; set; }
    }

    public class AddressUtxoModel : AddressModel, IAddressUtxo
    {
        public int BlockHeight { get; set; }
        public int TxIndex { get; set; }
        public Money Value { get; set; } = NullMoney;
        public uint256 TxSpentId { get; set; }
        public int TxSpentHeight { get; set; }
    }
}