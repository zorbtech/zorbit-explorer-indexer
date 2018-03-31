using System;
using System.Threading.Tasks;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class IndexerTransactionRepository : ITransactionRepository
    {
        public IndexerTransactionRepository(IndexerConfiguration config)
        {
            Configuration = config ?? throw new ArgumentNullException("config");
        }

        public async Task<Transaction> GetAsync(uint256 txId)
        {
            var tx = await Configuration.CreateIndexerClient().GetTransactionAsync(false, txId).ConfigureAwait(false);
            return tx?.Transaction;
        }

        public Task PutAsync(uint256 txId, Transaction tx)
        {
            Configuration.CreateIndexer().Index(new TransactionEntry.Entity(txId, tx, null));
            return Task.FromResult(false);
        }

        public IndexerConfiguration Configuration { get; }
    }
}
