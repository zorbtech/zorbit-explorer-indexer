using System;
using System.Collections.Generic;
using System.Text;

namespace Stratis.Bitcoin.Features.AzureIndexer.Balance
{
    public class FastEncoder
    {
        private static readonly FastEncoder _Instance = new FastEncoder();

        public static FastEncoder Instance => _Instance;

        public FastEncoder()
        {
            var ranges = new int[][]{
                new int[]{0x20,0x22},
                new int[]{0x26,0x2C},
                new int[]{0x30,0x3E},
                new int[]{0x40,0x5B},
                new int[]{0x5D,0x7E},
                new int[]{0xA0,0x148}
             };
            var unicodes = Enumerate(ranges);
            var builder = new StringBuilder(260);
            foreach (var i in unicodes)
            {
                builder.Append((char)i);
            }

            _bytesToChar = builder.ToString().ToCharArray();
            _charToBytes = new byte[ranges[ranges.Length - 1][1] + 1];

            var enumerator = unicodes.GetEnumerator();
            for (var i = 0 ; i < 256 ; i++)
            {
                enumerator.MoveNext();
                _charToBytes[enumerator.Current] = (byte)i;
            }
        }

        private IEnumerable<int> Enumerate(int[][] ranges)
        {
            foreach (var range in ranges)
            {
                for (var i = range[0] ; i <= range[1] ; i++)
                    yield return i;
            }
        }

        private readonly char[] _bytesToChar;
        private readonly byte[] _charToBytes;
        public byte[] DecodeData(string encoded)
        {
            var result = new byte[encoded.Length];
            var i = 0;
            foreach (var c in encoded.ToCharArray())
            {
                result[i] = _charToBytes[c];
                i++;
            }
            return result;
        }

        public string EncodeData(byte[] data, int offset, int length)
        {
            var result = new char[length];
            for (var i = 0 ; i < length ; i++)
            {
                result[i] = _bytesToChar[data[offset + i]];
            }
            return new String(result);
        }

        public string EncodeData(byte[] data)
        {
            return EncodeData(data, 0, data.Length);
        }
    }
}
