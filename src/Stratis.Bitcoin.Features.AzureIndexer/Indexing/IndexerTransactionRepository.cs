using System;
using System.Threading.Tasks;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class IndexerTransactionRepository : ITransactionRepository
    {
        private readonly IndexerConfiguration _configuration;
        public IndexerConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }
        public IndexerTransactionRepository(IndexerConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            _configuration = config;
        }
        #region ITransactionRepository Members

        public async Task<Transaction> GetAsync(uint256 txId)
        {
            var tx = await _configuration.CreateIndexerClient().GetTransactionAsync(false, txId).ConfigureAwait(false);
            if (tx == null)
                return null;
            return tx.Transaction;
        }

        public Task PutAsync(uint256 txId, Transaction tx)
        {
            _configuration.CreateIndexer().Index(new TransactionEntry.Entity(txId, tx, null));
            return Task.FromResult(false);
        }

        #endregion
    }
}
