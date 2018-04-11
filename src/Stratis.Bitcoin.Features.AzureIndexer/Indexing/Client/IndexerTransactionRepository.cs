using System;
using System.Threading.Tasks;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class IndexerTransactionRepository : ITransactionRepository
    {
        private readonly FullNode _fullNode;


        public IndexerTransactionRepository(
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            _fullNode = fullNode;
            Chain = chain;
            StorageClient = storageClient ?? throw new ArgumentNullException("storageClient");
            Settings = settings;
        }

        public async Task<Transaction> GetAsync(uint256 txId)
        {
            var tx = await new IndexerClient(_fullNode, Chain, StorageClient, Settings).GetTransactionAsync(false, txId).ConfigureAwait(false);
            return tx?.Transaction;
        }

        public Task PutAsync(uint256 txId, Transaction tx)
        {
            return new AzureIndexerObsolete(_fullNode, Chain, StorageClient, Settings).IndexTransactionsAsync(new TransactionEntry.Entity(txId, tx, null));
        }

        public ConcurrentChain Chain { get; }

        public AzureStorageClient StorageClient { get; }

        public AzureIndexerSettings Settings { get; }
    }
}
