using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class AzureStorageClient
    {
        private const string IndexerBlobContainerName = "indexer";
        private const string BlocksTableName = "blocks";
        private const string BlocksSummaryTableName = "summary";
        private const string TransactionsTableName = "transactions";
        private const string BalancesTableName = "balances";
        private const string ChainTableName = "chain";
        private const string WalletsTableName = "wallets";

        private readonly ILogger _logger;
        private readonly AzureIndexerSettings _settings;
        private CloudTableClient _tableClient;
        private CloudBlobClient _blobClient;
        private CloudStorageAccount _storageAccount;

        public AzureStorageClient(
            IFullNode node,
            AzureIndexerSettings settings,
            ILoggerFactory loggerFactory)
        {
            Network = node.Network;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<AzureStorageClient>();
        }

        public async Task InitaliseAsync()
        {
            if (this._settings.ResetStorage)
            {
                this._logger.LogInformation("Resetting Azure Storage");
                await TeardownAsync();
            }

            await EnsureSetupAsync();

            if (this._settings.ResetStorage)
            {
                this._logger.LogInformation("Azure Storage Reset");
            }
        }

        public IEnumerable<CloudTable> EnumerateTables()
        {
            yield return GetBlockTable();
            yield return GetBlockSummaryTable();
            yield return GetTransactionTable();
            yield return GetBalanceTable();
            yield return GetChainTable();
            yield return GetWalletRulesTable();
        }

        public CloudTable GetBlockTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(BlocksTableName));
        }

        public CloudTable GetBlockSummaryTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(BlocksSummaryTableName));
        }

        public CloudTable GetTransactionTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(TransactionsTableName));
        }

        public CloudTable GetWalletRulesTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(WalletsTableName));
        }

        public CloudTable GetBalanceTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(BalancesTableName));
        }

        public CloudTable GetChainTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(ChainTableName));
        }

        public CloudBlobContainer GetBlocksContainer()
        {
            return this.BlobClient.GetContainerReference(this.GetFullName(IndexerBlobContainerName));
        }

        private string GetFullName(string storageObjectName)
        {
            return (_settings.StorageNamespace + storageObjectName).ToLowerInvariant();
        }

        private Task TeardownAsync()
        {
            this.SetStorageAccount();
            var tasks = EnumerateTables()
                .Select(t => t.DeleteIfExistsAsync())
                .OfType<Task>()
                .ToList();
            tasks.Add(GetBlocksContainer().DeleteIfExistsAsync());
            return Task.WhenAll(tasks.ToArray());
        }

        private Task EnsureSetupAsync()
        {
            this.SetStorageAccount();
            var tasks = EnumerateTables()
                .Select(t => t.SafeCreateIfNotExistsAsync())
                .OfType<Task>()
                .ToList();
            tasks.Add(GetBlocksContainer().SafeCreateIfNotExistsAsync());
            return Task.WhenAll(tasks.ToArray());
        }

        private void SetStorageAccount()
        {
            if (_storageAccount != null)
            {
                return;
            }

            this._storageAccount = _settings.AzureEmulatorUsed ?
                CloudStorageAccount.Parse("UseDevelopmentStorage=true;") :
                CloudStorageAccount.Parse(_settings.AzureConnectionString);
        }

        public CloudTableClient TableClient
        {
            get
            {
                if (this._tableClient != null)
                {
                    return this._tableClient;
                }
                this.SetStorageAccount();
                this._tableClient = this._storageAccount.CreateCloudTableClient();
                return this._tableClient;
            }
        }
        
        public CloudBlobClient BlobClient
        {
            get
            {
                if (this._blobClient != null)
                {
                    return this._blobClient;
                }
                this.SetStorageAccount();
                this._blobClient = this._storageAccount.CreateCloudBlobClient();
                return this._blobClient;
            }
        }

        public Network Network { get; }
    }
}
