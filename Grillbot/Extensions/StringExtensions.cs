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

        public static string FormatDiscordUrl(this string str)
        {
            if (Uri.IsWellFormedUriString(str, UriKind.Absolute))
                return $"<{str}>";

            return str;
        }

        public static string GetMiddle(this string src, string start, string finish)
        {
            if (!src.Contains(start) || !src.Contains(finish)) return string.Empty;

            var beginning = src.IndexOf(start, 0, StringComparison.Ordinal) + start.Length;
            var end = src.IndexOf(finish, beginning, StringComparison.Ordinal);
            return src.Substring(beginning, end - beginning);
        }

        public static string PreventMassTags(this string str)
        {
            if (!str.Contains("everyone") && !str.Contains("here"))
                return str;

            return str.Replace("@everyone", "@ everyone").Replace("@here", "@ here");
        }
    }
}
