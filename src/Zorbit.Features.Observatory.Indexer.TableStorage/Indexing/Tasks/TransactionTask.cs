using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Adapters;

namespace Zorbit.Features.Observatory.TableStorage.Indexing.Tasks
{
    public class TransactionTask : IndexerTableTask
    {
        public TransactionTask(
            AzureStorageClient storageClient,
            IndexerSettings settings)
            : base(storageClient, settings)
        {
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetTransactionTable();
        }

        protected override Task<IEnumerable<ITaskAdapter>> GetTasksAsync(IEnumerable<IBlockInfo> blocks)
        {
            var adapters = new List<ITaskAdapter>();
            foreach (var block in blocks)
            {
                adapters.AddRange(block.Block.Transactions.Select(tx => new TransactionAdapter(new TransactionModel(block.Hash, tx))));
            }
            IEnumerable<ITaskAdapter> result = new List<ITaskAdapter>(adapters);
            return Task.FromResult(result);
        }
    }
}