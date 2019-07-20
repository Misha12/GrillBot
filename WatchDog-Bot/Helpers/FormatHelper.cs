using System;
using System.Globalization;

namespace WatchDog_Bot.Helpers
{
    public static class FormatHelper
    {
        private const string SpacedNumberFormat = "#,0";

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

        public static string Format(ulong number, string format) => number.ToString(format, GetFormatInfo());
        public static string Format(int number, string format) => number.ToString(format, GetFormatInfo());

        public static string FormatWithSpaces(int number) => Format(number, SpacedNumberFormat);
        public static string FormatWithSpaces(uint number) => Format(number, SpacedNumberFormat);
        public static string FormatWithSpaces(ulong number) => Format(number, SpacedNumberFormat);

        private static NumberFormatInfo GetFormatInfo()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            return nfi;
        }

    }
}
