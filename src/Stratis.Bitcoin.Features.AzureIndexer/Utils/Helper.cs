using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Newtonsoft.Json;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    internal static class Helper
    {
        internal static List<T> DeserializeList<T>(byte[] bytes) where T : IBitcoinSerializable, new()
        {
            var outpoints = new List<T>();
            if (bytes == null)
            {
                return outpoints;
            }

            using (var ms = new MemoryStream(bytes) {Position = 0})
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
        internal static void SetThrottling()
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
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

        private const int ColumnMaxSize = 63000;

        internal static void SetEntityProperty(DynamicTableEntity entity, string property, byte[] data)
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

        internal static byte[] GetEntityProperty(DynamicTableEntity entity, string property)
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

        internal static string GetPartitionKey(int bits, byte[] bytes, int startIndex, int length)
        {
            ulong result = 0;
            var remainingBits = bits;
            for (var i = 0; i < length; i++)
            {
                var taken = Math.Min(8, remainingBits);
                var inc = (bytes[startIndex + i] & ~(0xFFUL >> taken)) << (i * 8);
                result = result + inc;
                remainingBits -= taken;
                if (remainingBits == 0)
                    break;
            }
            return result.ToString("X2");
        }

        private static JsonSerializerSettings _settings;

        internal static JsonSerializerSettings Settings
        {
            get
            {
                if (_settings != null)
                {
                    return _settings;
                }

                _settings = new JsonSerializerSettings();
                _settings.Converters.Add(new ScriptJsonConverter());
                _settings.Converters.Add(new WalletRuleConverter());
                return _settings;
            }
        }

        internal static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        internal static T DeserializeObject<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str, Settings);
        }

        public static bool IsError(Exception ex, string code)
        {
            var actualCode = (ex as StorageException)?.RequestInformation?.ExtendedErrorInformation?.ErrorCode;
            return actualCode == code;
        }

        internal static string Format = new string(Enumerable.Range(0, int.MaxValue.ToString().Length).Select(c => '0').ToArray());

        private static readonly char[] Digit = Enumerable.Range(0, 10).Select(c => c.ToString()[0]).ToArray();

        //Convert '012' to '987'
        internal static string HeightToString(int height)
        {
            var input = height.ToString(Format);
            return ToggleChars(input);
        }

        internal static string ToggleChars(string input)
        {
            var result = new char[input.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var index = Array.IndexOf(Digit, input[i]);
                result[i] = Digit[Digit.Length - index - 1];
            }
            return new string(result);
        }

        //Convert '987' to '012'
        internal static int StringToHeight(string rowkey)
        {
            return int.Parse(ToggleChars(rowkey));
        }

        public static string GetPartitionKey(int bits, uint nbr)
        {
            var bytes = BitConverter.GetBytes(nbr);
            return GetPartitionKey(bits, bytes, 0, 4);
        }
    }
}
