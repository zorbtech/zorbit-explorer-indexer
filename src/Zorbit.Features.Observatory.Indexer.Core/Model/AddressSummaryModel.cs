using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IAddressSummary : IAddress
    {
        Money Balance { get; set; }
        Money Sent { get; set; }
        Money Received { get; set; }
        Money Staked { get; set; }
        int TxCount { get; set; }
    }

    public class AddressSummaryModel : AddressModel, IAddressSummary
    {
        public AddressSummaryModel()
        {
            Kind = AddressKind.Summary;
        }

        public Money Balance { get; set; } = NullMoney;
        public Money Sent { get; set; } = NullMoney;
        public Money Received { get; set; } = NullMoney;
        public Money Staked { get; set; } = NullMoney;
        public int TxCount { get; set; }
    }

    public enum AddressKind
    {
        Summary,
        Transaction,
        Utxo
    }

    public enum TransactionType
    {
        CoinBase,
        CoinStake,
        Normal
    }

    public interface IAddress
    {
        AddressKind Kind { get; set; }
        string Address { get; set; }
    }

    public abstract class AddressModel : IAddress
    {
        protected static readonly Money NullMoney = new Money(0);

        public AddressKind Kind { get; set; }
        public string Address { get; set; }
    }
}