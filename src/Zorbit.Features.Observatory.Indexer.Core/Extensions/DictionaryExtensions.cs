using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Zorbit.Features.Observatory
{
    public static class DictionaryExtensions
    {
        public static bool TryUpdate<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue, TValue> updateFactory)
        {
            while (dict.TryGetValue(key, out var curValue))
            {
                if (dict.TryUpdate(key, updateFactory(curValue), curValue))
                    return true;
            }
            return false;
        }
    }
}
