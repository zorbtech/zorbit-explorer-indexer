using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Newtonsoft.Json;

namespace Zorbit.Features.Observatory.TableStorage.Utils
{
    public static class Helper
    {
        private const int ColumnMaxSize = 63000;
        
        public static List<T> DeserializeList<T>(byte[] bytes) where T : IBitcoinSerializable, new()
        {
            var outpoints = new List<T>();
            if (bytes == null)
            {
                return outpoints;
            }

            using (var ms = new MemoryStream(bytes) { Position = 0 })
            {
                while (ms.Position != ms.Length)
                {
                    var outpoint = new T();
                    outpoint.ReadWrite(ms, false);
                    outpoints.Add(outpoint);
                }
            }

            return outpoints;
        }

        public static byte[] SerializeList<T>(IEnumerable<T> items) where T : IBitcoinSerializable
        {
            using (var ms = new MemoryStream())
            {
                foreach (var item in items)
                {
                    item.ReadWrite(ms, true);
                }

                return GetBytes(ms) ?? new byte[0];
            }
        }

        public static byte[] GetBytes(MemoryStream stream)
        {
            if (stream.Length == 0)
            {
                return null;
            }

            var buffer = stream.GetBuffer();
            Array.Resize(ref buffer, (int)stream.Length);
            return buffer;
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }

        public static bool IsError(Exception ex, string code)
        {
            var actualCode = (ex as StorageException)?.RequestInformation?.ExtendedErrorInformation?.ErrorCode;
            return actualCode == code;
        }

        public static void SetEntityProperty(DynamicTableEntity entity, string property, byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

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
                entity.Properties.AddOrReplace(property + i, new EntityProperty(chunk));
                i++;
            }
        }

        public static byte[] GetEntityProperty(DynamicTableEntity entity, string property)
        {
            var chunks = new List<byte[]>();
            var i = 0;
            while (true)
            {
                if (!entity.Properties.ContainsKey(property + i))
                    break;
                var chunk = entity.Properties[property + i].BinaryValue;
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
    }
}
