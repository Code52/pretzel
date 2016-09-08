using System.Collections.Generic;

namespace Pretzel.Logic.Extensions
{
    /// <summary>
    /// Dictionary extension methods.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merges two dictionaries on top of each other and returns a new dictionary.
        /// Values from the second override the original values when the key is already present.
        /// Values from the second will be added when the key is not present in the first.
        /// </summary>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            var result = new Dictionary<TKey,TValue>(first);
            if (second != null)
            {
                foreach (var key in second.Keys)
                {
                    result[key] = second[key];
                }
            }
            return result;
        }
    }
}
