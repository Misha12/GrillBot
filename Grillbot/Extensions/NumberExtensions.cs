using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
