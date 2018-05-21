using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace Zorbit.Features.Observatory.TableStorage.Utils
{
    public static class Extensions
    {
        public static async Task<bool> SafeCreateIfNotExistsAsync(this CloudTable table, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            return await SafeCreateIfNotExistsAsync(table, CancellationToken.None, requestOptions, operationContext);
        }

        public static async Task<bool> SafeCreateIfNotExistsAsync(this CloudBlobContainer table)
        {
            return await SafeCreateIfNotExistsAsync(table, CancellationToken.None);
        }

        public static async Task<bool> SafeCreateIfNotExistsAsync(this CloudTable table, CancellationToken cancellationToken, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var created = await table.CreateIfNotExistsAsync(requestOptions, operationContext, cancellationToken);
                    return created;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode == 409 &&
                        e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted))
                    {
                        await Task.Delay(5000, cancellationToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> SafeCreateIfNotExistsAsync(this CloudBlobContainer table, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var created = await table.CreateIfNotExistsAsync();
                    return created;
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode == 409 &&
                        e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(BlobErrorCodeStrings.ContainerBeingDeleted))
                    {
                        await Task.Delay(5000, cancellationToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return false;
        }
    }
}
