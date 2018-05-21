using Microsoft.WindowsAzure.Storage.Table;

namespace Zorbit.Features.AzureIndexer.TableStorage
{
    public abstract class AzureTableEntity
    {
        protected string PartitionKey { get; }

        protected AzureTableEntity(string partitionKey)
        {
            PartitionKey = partitionKey?.ToLowerInvariant();
        }

        public abstract ITableEntity ToEntity();
    }
}