using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface ITransaction
    {
        uint256 BlockId { get; set; }
        Transaction Transaction { get; set; }
    }

    public class TransactionModel : ITransaction
    {
        public TransactionModel()
        {
        }

        public TransactionModel(uint256 blockId, Transaction transaction)
        {
            BlockId = blockId;
            Transaction = transaction;
        }

        public uint256 BlockId { get; set; }
        public Transaction Transaction { get; set; }
    }
}
