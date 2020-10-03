using System;
using System.Globalization;

namespace Grillbot.Extensions
{
    public static class NumberExtensions
    {
        private const string SpacedNumberFormat = "#,0";

        public static string Format(this int number, string format) => number.ToString(format, GetFormatInfo());
        public static string Format(this long number, string format) => number.ToString(format, GetFormatInfo());
        public static string Format(this ulong number, string format) => number.ToString(format, GetFormatInfo());

        public static string FormatWithSpaces(this int number) => Format(number, SpacedNumberFormat);
        public static string FormatWithSpaces(this uint number) => Format(number, SpacedNumberFormat);
        public static string FormatWithSpaces(this long number) => Format(number, SpacedNumberFormat);
        public static string FormatWithSpaces(this ulong number) => Format(number, SpacedNumberFormat);

        private static NumberFormatInfo GetFormatInfo()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            return nfi;
        }

        public static string FormatAsSize(this long bytes)
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
