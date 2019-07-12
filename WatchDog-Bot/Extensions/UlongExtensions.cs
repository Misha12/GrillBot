using System.Globalization;

namespace WatchDog_Bot.Extensions
{
    public static class UlongExtensions
    {
        public static string Format(this ulong number, string format)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            return number.ToString(format, nfi);
        }
    }
}
