using System;
using System.Collections.Generic;

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

        public static string GetMiddle(this string src, string start, string finish)
        {
            if (!src.Contains(start) || !src.Contains(finish)) return string.Empty;

            var beginning = src.IndexOf(start, 0, StringComparison.Ordinal) + start.Length;
            var end = src.IndexOf(finish, beginning, StringComparison.Ordinal);

            return src[beginning..end];
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
    }
}
