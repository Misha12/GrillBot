using System;
using System.Collections.Generic;

namespace Grillbot.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            return SplitParts(s, partLength);
        }

        public static IEnumerable<string> SplitParts(string s, int partLength)
        {
            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        public static string FormatDiscordUrl(this string str)
        {
            if (Uri.IsWellFormedUriString(str, UriKind.Absolute))
                return $"<{str}>";

            return str;
        }
        
        public static IEnumerable<string> SplitByLength(this string str, int maxLength) {
            for (int index = 0; index < str.Length; index += maxLength) {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }
    }
}
