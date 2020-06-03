using System;
using System.Globalization;

namespace Grillbot.Helpers
{
    public static class FormatHelper
    {
        public static string FormatAsSize(long bytes)
        {
            if (bytes < 0)
                throw new ArgumentException("Size must be positive");

            if (bytes == 0)
                return "0 B";

            string[] sizes = new[] { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            var factor = (int)Math.Floor((bytes.ToString().Length - 1) / 3.0D);
            var finalSize = bytes / Math.Pow(1024, factor);

            return $"{Math.Round(finalSize, 2)} {sizes[factor]}";
        }
    }
}