using System;
using System.Collections.Generic;

namespace Wexflow.CommandLineParserClient.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue ValueOrDefault<TValue>(this IDictionary<string, string> dictionary, string key, 
            Func<string, TValue> converter,
            TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out string value)
                ? converter(value)
                : defaultValue;
        }
    }
}