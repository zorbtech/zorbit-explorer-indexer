using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    internal class ReadOnlyTransactionRepository : ITransactionRepository
    {
        private readonly NoSqlTransactionRepository _cache;

        public ReadOnlyTransactionRepository(NoSqlTransactionRepository cache)
        {
            this._cache = cache;
        }
        #region ITransactionRepository Members

        public Task<Transaction> GetAsync(uint256 txId)
        {
            return _cache.GetAsync(txId);
        }

        public Task PutAsync(uint256 txId, Transaction tx)
        {
            return Task.FromResult(false);
        }

        #endregion
    }
    internal class CompositeTransactionRepository : ITransactionRepository
    {
        public CompositeTransactionRepository(ITransactionRepository[] repositories)
        {
            _repositories = repositories.ToArray();
        }

        private readonly ITransactionRepository[] _repositories;
        #region ITransactionRepository Members

        public async Task<Transaction> GetAsync(uint256 txId)
        {
            foreach(var repo in _repositories)
            {
                var result = await repo.GetAsync(txId).ConfigureAwait(false);
                if(result != null)
                    return result;
            }
            return null;
        }

        public async Task PutAsync(uint256 txId, Transaction tx)
        {
            foreach(var repo in _repositories)
            {
                await repo.PutAsync(txId, tx).ConfigureAwait(false);
            }
        }

        #endregion
    }
    public class IndexerColoredTransactionRepository : IColoredTransactionRepository
    {
        private readonly IndexerConfiguration _configuration;
        public IndexerConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        public IndexerColoredTransactionRepository(IndexerConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            _configuration = config;
            _transactions = new IndexerTransactionRepository(config);
        }

        #region IColoredTransactionRepository Members

        public async Task<ColoredTransaction> GetAsync(uint256 txId)
        {
            var client = _configuration.CreateIndexerClient();
            var tx = await client.GetTransactionAsync(false, false, txId).ConfigureAwait(false);
            if (tx == null)
                return null;
            return tx.ColoredTransaction;
        }

        public Task PutAsync(uint256 txId, ColoredTransaction colored)
        {
            _configuration.CreateIndexer().Index(new TransactionEntry.Entity(txId, colored));
            return Task.FromResult(false);
        }

        private ITransactionRepository _transactions;
        public ITransactionRepository Transactions
        {
            get
            {
                return _transactions;
            }
            set
            {
                _transactions = value;
            }
        }

        #endregion
    }
}
