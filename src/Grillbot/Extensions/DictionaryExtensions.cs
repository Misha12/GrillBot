using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
            {
                dict.Add(item.Key, item.Value);
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> anotherDict)
        {
            dict.AddRange(anotherDict.Select(o => o));
        }
    }
}
