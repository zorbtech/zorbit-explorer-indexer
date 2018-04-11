using System;
using System.Collections.Generic;
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

        public Task<Transaction> GetAsync(uint256 txId)
        {
            return _cache.GetAsync(txId);
        }

        public Task PutAsync(uint256 txId, Transaction tx)
        {
            return Task.FromResult(false);
        }
    }

    internal class CompositeTransactionRepository : ITransactionRepository
    {
        private readonly ITransactionRepository[] _repositories;

        public CompositeTransactionRepository(IEnumerable<ITransactionRepository> repositories)
        {
            _repositories = repositories.ToArray();
        }

        public async Task<Transaction> GetAsync(uint256 txId)
        {
            foreach (var repo in _repositories)
            {
                var result = await repo.GetAsync(txId).ConfigureAwait(false);
                if (result != null)
                    return result;
            }

            return null;
        }

        public async Task PutAsync(uint256 txId, Transaction tx)
        {
            foreach (var repo in _repositories)
            {
                await repo.PutAsync(txId, tx).ConfigureAwait(false);
            }
        }
    }

    public class IndexerColoredTransactionRepository : IColoredTransactionRepository
    {
        public IndexerColoredTransactionRepository(
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient, 
            AzureIndexerSettings settings)
        {
            FullNode = fullNode;
            Chain = chain;
            StorageClient = storageClient ?? throw new ArgumentNullException("storageClient");
            Settings = settings;
            Transactions = new IndexerTransactionRepository(FullNode, Chain, storageClient, settings);
        }

        public async Task<ColoredTransaction> GetAsync(uint256 txId)
        {
            var client = new IndexerClient(FullNode, Chain, StorageClient, Settings);
            var tx = await client.GetTransactionAsync(false, false, txId).ConfigureAwait(false);
            return tx?.ColoredTransaction;
        }

        public Task PutAsync(uint256 txId, ColoredTransaction colored)
        {
            return new AzureIndexerObsolete(FullNode, Chain, StorageClient, Settings).IndexTransactionsAsync(new TransactionEntry.Entity(txId, colored));
        }

        public FullNode FullNode { get; }

        public ConcurrentChain Chain { get; }

        public AzureStorageClient StorageClient { get; }

        public AzureIndexerSettings Settings { get; }

        public ITransactionRepository Transactions { get; set; }
    }
}
