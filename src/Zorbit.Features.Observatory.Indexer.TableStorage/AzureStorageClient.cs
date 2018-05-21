using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.TableStorage.Utils;

namespace Zorbit.Features.Observatory.TableStorage
{
    public class AzureStorageClient
    {
        private const string CheckpointTableName = "checkpoints";
        private const string BlocksTableName = "blocks";
        private const string BlocksSummaryTableName = "summary";
        private const string TransactionsTableName = "transactions";
        private const string AddressTableName = "addresses";

        private readonly ILogger _logger;
        private readonly IndexerSettings _settings;
        private CloudTableClient _tableClient;
        private CloudStorageAccount _storageAccount;

        public AzureStorageClient(
            IFullNode node,
            IndexerSettings settings,
            ILoggerFactory loggerFactory)
        {
            Network = node.Network;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<AzureStorageClient>();
            SetStorageDefaults();
        }

        public async Task InitaliseAsync()
        {
            if (this._settings.ResetStorage)
            {
                this._logger.LogInformation("Resetting Azure Storage");
                await DeleteTablesAsync();
            }
            
            await CreateTablesAsync();

            if (this._settings.ResetStorage)
            {
                this._logger.LogInformation("Azure Storage Reset");
            }
        }

        public IEnumerable<CloudTable> EnumerateTables()
        {
            yield return GetCheckpointTable();
            yield return GetBlockTable();
            yield return GetBlockSummaryTable();
            yield return GetTransactionTable();
            yield return GetAddressTable();
        }

        public CloudTable GetCheckpointTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(CheckpointTableName));
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
        
        public CloudTable GetAddressTable()
        {
            return this.TableClient.GetTableReference(this.GetFullName(AddressTableName));
        }

        private static void SetStorageDefaults()
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
        }

        private string GetFullName(string storageObjectName)
        {
            return (_settings.StorageNamespace + storageObjectName).ToLowerInvariant();
        }

        private Task DeleteTablesAsync()
        {
            this.SetStorageAccount();
            var tasks = EnumerateTables()
                .Select(t => t.DeleteIfExistsAsync())
                .OfType<Task>()
                .ToList();
            return Task.WhenAll(tasks.ToArray());
        }

        private Task CreateTablesAsync()
        {
            this.SetStorageAccount();
            var tasks = EnumerateTables()
                .Select(t => t.SafeCreateIfNotExistsAsync())
                .OfType<Task>()
                .ToList();
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

        public Network Network { get; }
    }
}
