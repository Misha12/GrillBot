using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach(var element in source)
            {
                if(seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> SplitInParts<T>(this IEnumerable<T> source, int partSize)
        {
            for (int i = 0; i < Math.Ceiling((double)source.Count() / partSize); i++)
                yield return source.Skip(i * partSize).Take(partSize);
        }
    }
}
