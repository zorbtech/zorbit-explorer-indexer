using System;
using System.Linq;

namespace Zorbit.Features.Observatory.Core.Extensions
{
    public static class StringExtensions
    {
        internal static string Format => new string(Enumerable.Range(0, int.MaxValue.ToString().Length).Select(c => '0').ToArray());
        private static readonly char[] Digit = Enumerable.Range(0, 10).Select(c => c.ToString()[0]).ToArray();

        public static string Partition(this int source)
        {
            var sourceStr = source.ToString("D4");
            return string.IsNullOrEmpty(sourceStr) ? sourceStr : sourceStr.Partition();
        }

        public static string Partition(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var lastChars = source.Substring(source.Length - 3);
            return GetStringValue(lastChars).ToString("D4");
        }

        /// <summary>
        /// Convert '012' to '987'
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string ToDecendingString(this int value)
        {
            var input = value.ToString(Format);
            return ToggleChars(input);
        }

        /// <summary>
        /// Convert '987' to '012'
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToD(this string value)
        {
            return int.Parse(ToggleChars(value));
        }

        public static string ToggleChars(string input)
        {
            var result = new char[input.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var index = Array.IndexOf(Digit, input[i]);
                result[i] = Digit[Digit.Length - index - 1];
            }
            return new string(result);
        }

        /// <summary>
        /// Converts string into a base 10 value
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static int GetStringValue(string source)
        {
            var result = 0;

            if (string.IsNullOrEmpty(source))
                return result;

            var chars = source.ToCharArray();
            foreach (var c in chars)
            {
                if (int.TryParse(c.ToString(), out var lastInt))
                {
                    result += lastInt;
                }
                else
                {
                    result += 9 + c % 32;
                }
            }

            return result;
        }
    }
}
