using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class IndexerConfiguration
    {
        private const string IndexerBlobContainerName = "indexer";
        private const string TransactionsTableName = "transactions";
        private const string BalancesTableName = "balances";
        private const string ChainTableName = "chain";
        private const string WalletsTableName = "wallets";

        private CloudTableClient _tableClient;
        private CloudBlobClient _blobClient;

        public IndexerConfiguration()
        {
            Network = Network.Main;
        }

        public IndexerConfiguration(IConfiguration config)
        {
            this.StorageNamespace = GetValue(config, "StorageNamespace", false);
            var network = GetValue(config, "Bitcoin.Network", false) ?? "Main";
            this.Network = Network.GetNetwork(network);
            if (this.Network == null)
                throw new IndexerConfigurationErrorsException(
                    $"Invalid value {network} in appsettings (expecting Main, Test or Seg)");
            this.Node = GetValue(config, "Node", false);
            this.CheckpointSetName = GetValue(config, "CheckpointSetName", false);
            if (string.IsNullOrWhiteSpace(this.CheckpointSetName))
                this.CheckpointSetName = "default";

            var emulator = GetValue(config, "AzureStorageEmulatorUsed", false);
            if (!string.IsNullOrWhiteSpace(emulator))
                this.AzureStorageEmulatorUsed = bool.Parse(emulator);

            this.AzureConnectionString = GetValue(config, "AzureConnectionString", false);
        }

        public Task EnsureSetupAsync()
        {
            var tasks = EnumerateTables()
                .Select(t => t.CreateIfNotExistsAsync())
                .OfType<Task>()
                .ToList();
            tasks.Add(GetBlocksContainer().CreateIfNotExistsAsync());
            return Task.WhenAll(tasks.ToArray());
        }

        public void EnsureSetup()
        {
            try
            {
                this.StorageAccount = this.AzureStorageEmulatorUsed ?
                    CloudStorageAccount.Parse("UseDevelopmentStorage=true;") :
                    CloudStorageAccount.Parse(AzureConnectionString);

                EnsureSetupAsync().Wait();
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex).Throw();
                throw;
            }
        }

        public IEnumerable<CloudTable> EnumerateTables()
        {
            yield return GetTransactionTable();
            yield return GetBalanceTable();
            yield return GetChainTable();
            yield return GetWalletRulesTable();
        }

        public AzureIndexer CreateIndexer()
        {
            return new AzureIndexer(this);
        }

        public IndexerClient CreateIndexerClient()
        {
            return new IndexerClient(this);
        }

        public CloudTable GetTransactionTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(TransactionsTableName));
        }

        public CloudTable GetWalletRulesTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(WalletsTableName));
        }

        public CloudTable GetTable(string tableName)
        {
            return this.TableClient.GetTableReference(this.GetFullName(tableName));
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
            return (StorageNamespace + storageObjectName).ToLowerInvariant();
        }

        protected static string GetValue(IConfiguration config, string setting, bool required)
        {
            var result = config[setting];
            result = String.IsNullOrWhiteSpace(result) ? null : result;
            if (result == null && required)
                throw new IndexerConfigurationErrorsException($"AppSetting {setting} not found");
            return result;
        }

        public CloudTableClient TableClient
        {
            get
            {
                if (this._tableClient != null)
                {
                    return this._tableClient;
                }
                this._tableClient = this.StorageAccount.CreateCloudTableClient();
                return this._tableClient;
            }
            set => this._tableClient = value;
        }


        public CloudBlobClient BlobClient
        {
            get
            {
                if (this._blobClient != null)
                {
                    return this._blobClient;
                }
                this._blobClient = this.StorageAccount.CreateCloudBlobClient();
                return this._blobClient;
            }
            set => this._blobClient = value;
        }

        public Network Network { get; set; }

        public string AzureConnectionString { get; set; }

        public bool AzureStorageEmulatorUsed { get; set; }

        public string Node { get; set; }

        public string CheckpointSetName { get; set; }

        public string StorageNamespace { get; set; }

        public CloudStorageAccount StorageAccount { get; set; }
    }
}
