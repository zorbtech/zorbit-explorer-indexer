using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Zorbit.Features.Observatory.TableStorage.Utils
{
    public static class TableExtensions
    {
        private const int ColumnMaxSize = 64000;

        /// <summary>
        /// Gets the size of the adapter in bytes. Calculates based from
        /// https://blogs.msdn.microsoft.com/avkashchauhan/2011/11/30/how-the-size-of-an-entity-is-caclulated-in-windows-azure-table-storage/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public static int GetAdapterSize<T>(this TableEntityAdapter<T> adapter)
        {

            var messageSize = 4; // Message Overhead
            messageSize += !string.IsNullOrEmpty(adapter.PartitionKey) ? adapter.PartitionKey.Length * 2 : 0;
            messageSize += !string.IsNullOrEmpty(adapter.RowKey) ? adapter.RowKey.Length + 2 : 0;

            var properties = adapter.WriteEntity(new OperationContext());
            foreach (var kp in properties)
            {
                var propertySize = 8; // Property Overhead

                propertySize += kp.Key.Length * 2;

                switch (kp.Value.PropertyType)
                {
                    case EdmType.String:
                        propertySize += !string.IsNullOrEmpty(kp.Value.StringValue) ? kp.Value.StringValue.Length * 2 + 4 : 0;
                        break;
                    case EdmType.Binary:
                        propertySize += kp.Value.BinaryValue?.Length + 4 ?? 0;
                        break;
                    case EdmType.Boolean:
                        propertySize += kp.Value.BooleanValue.HasValue ? 1 : 0;
                        break;
                    case EdmType.DateTime:
                        propertySize += kp.Value.DateTime.HasValue ? 8 : 0;
                        break;
                    case EdmType.Double:
                        propertySize += kp.Value.DoubleValue.HasValue ? 8 : 0;
                        break;
                    case EdmType.Guid:
                        propertySize += kp.Value.GuidValue.HasValue ? 16 : 0;
                        break;
                    case EdmType.Int32:
                        propertySize += kp.Value.Int32Value.HasValue ? 4 : 0;
                        break;
                    case EdmType.Int64:
                        propertySize += kp.Value.Int64Value.HasValue ? 8 : 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                messageSize += propertySize;
            }

            return messageSize;
        }

        public static IDictionary<string, EntityProperty> ByteArrayToProperties(this byte[] data, string propertyName)
        {
            var result = new Dictionary<string, EntityProperty>();
            if (data == null || data.Length == 0)
            {
                return result;
            }

            var remaining = data.Length;
            var offset = 0;
            var i = 0;

            while (remaining != 0)
            {
                var chunkSize = Math.Min(ColumnMaxSize, remaining);
                remaining -= chunkSize;
                var chunk = new byte[chunkSize];
                Array.Copy(data, offset, chunk, 0, chunkSize);
                offset += chunkSize;
                result.Add($"{propertyName}{i}", new EntityProperty(chunk));
                i++;
            }

            return result;
        }

        public static byte[] PropertiesToByteArray(this IDictionary<string, EntityProperty> properties, string property)
        {
            var chunks = new List<byte[]>();
            var i = 0;
            while (true)
            {
                if (!properties.ContainsKey(property + i))
                    break;
                var chunk = properties[property + i].BinaryValue;
                if (chunk == null || chunk.Length == 0)
                    break;
                chunks.Add(chunk);
                i++;
            }
            var data = new byte[chunks.Sum(o => o.Length)];
            var offset = 0;
            foreach (var chunk in chunks)
            {
                Array.Copy(chunk, 0, data, offset, chunk.Length);
                offset += chunk.Length;
            }
            return data;
        }

        public static byte[] PropertiesToString(this IDictionary<string, EntityProperty> properties, string property)
        {
            var chunks = new List<byte[]>();
            var i = 0;
            while (true)
            {
                if (!properties.ContainsKey(property + i))
                    break;
                var chunk = properties[property + i].BinaryValue;
                if (chunk == null || chunk.Length == 0)
                    break;
                chunks.Add(chunk);
                i++;
            }
            var data = new byte[chunks.Sum(o => o.Length)];
            var offset = 0;
            foreach (var chunk in chunks)
            {
                Array.Copy(chunk, 0, data, offset, chunk.Length);
                offset += chunk.Length;
            }
            return data;
        }

        public static IDictionary<string, EntityProperty> GetProperties(this IList<byte[]> data, string propertyName)
        {
            var result = new Dictionary<string, EntityProperty>();
            if (!data.Any())
            {
                return result;
            }

            for (var i = 0; i < data.Count; i++)
            {
                result.Add($"{propertyName}{i}", new EntityProperty(data[i]));
            }

            return result;
        }

        public static IList<byte[]> GetProperty(this IDictionary<string, EntityProperty> properties, string property)
        {
            var chunks = new List<byte[]>();
            var i = 0;

            while (true)
            {
                if (!properties.ContainsKey(property + i))
                {
                    break;
                }

                var chunk = properties[property + i].BinaryValue;
                if (chunk == null || chunk.Length == 0)
                {
                    break;
                }

                chunks.Add(chunk);
                i++;
            }

            return chunks;
        }
    }
}
