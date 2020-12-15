using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            return SplitParts(s, partLength);
        }

        public static IEnumerable<string> SplitParts(this string s, int partLength)
        {
            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string PreventMassTags(this string str)
        {
            return !str.Contains("@") ? str : str.Replace("@", "@ ");
        }

        public static string Cut(this string str, int maxLength)
        {
            if (str.Length >= maxLength - 3)
                str = str.Substring(0, maxLength - 3) + "...";

            return str;
        }

        public static string ClearCodeBlocks(this string str)
        {
            if (str.StartsWith("```"))
                str = str[3..];

            if (str.EndsWith("```"))
                str = str[0..^3];

            return str;
        }

        public static bool TranslateCzToBool(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return str.ToLower() == "ano";
        }

        public static string Repeat(this string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }
    }
}
