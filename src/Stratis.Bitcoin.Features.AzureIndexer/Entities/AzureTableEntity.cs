namespace Stratis.Bitcoin.Features.AzureIndexer.Entities
{
    public abstract class AzureTableEntity
    {
        protected string PartitionKey { get; }

        protected AzureTableEntity(string partitionKey)
        {
            PartitionKey = partitionKey?.ToLowerInvariant();
        }
    }
}