using System.Collections.Generic;
using System.Linq;

namespace Zorbit.Features.Observatory.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
        {
            for (var i = 0; i < (float)source.Count() / size; i++)
            {
                yield return source.Skip(i * size).Take(size);
            }
        }
    }
}
