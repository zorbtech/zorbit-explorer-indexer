using System;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IAddressTransaction : IAddress
    {
        uint256 TxId { get; set; }
        uint256 BlockId { get; set; }
        int BlockHeight { get; set; }
        DateTimeOffset Time { get; set; }
        Money Value { get; set; }
        TransactionType TxType { get; set; }
        Money TxBalance { get; set; }
    }

    public class AddressTransactionModel : AddressModel, IAddressTransaction
    {
        public AddressTransactionModel()
        {
            Kind = AddressKind.Transaction;
        }

        public uint256 TxId { get; set; }
        public uint256 BlockId { get; set; }
        public int BlockHeight { get; set; }
        public DateTimeOffset Time { get; set; }
        public Money Value { get; set; } = NullMoney;
        public TransactionType TxType { get; set; }
        public Money TxBalance { get; set; } = NullMoney;
    }
}