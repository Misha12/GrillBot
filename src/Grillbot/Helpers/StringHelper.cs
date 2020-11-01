using System;
using System.Globalization;
using System.Text;

namespace Grillbot.Helpers
{
    public static class StringHelper
    {
        public static string CreateRandomString(int length)
        {
            if (length == 0) return "";

            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var str = new StringBuilder();
            var random = new Random();

            int randomValue;
            for (int i = 0; i < length; i++)
            {
                randomValue = random.Next(0, alphabet.Length);
                str.Append(alphabet[randomValue]);
            }

            return str.ToString();
        }

        public static DateTime ParseDateTime(string dateTime, out string format)
        {
            var formats = new[]
            {
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd. MM. yyyy HH:mm",
                "dd. MM. yyyy HH:mm:ss",
                "dd/MM/yyyy"
            };

            var dt = TryParseDateTimeExact(dateTime, formats, out string outFormat);

            if (dt == null)
            {
                if (DateTime.TryParse(dateTime, out DateTime dtVal))
                {
                    format = "ISO8601";
                    return dtVal;
                }

                throw new FormatException("Cannot parse DateTime");
            }

            format = outFormat;
            return dt.Value;
        }

        public static DateTime ParseDateTime(string dateTime)
        {
            return ParseDateTime(dateTime, out string _);
        }

        public static DateTime? TryParseDateTimeExact(string dateTime, string[] formats, out string outputFormat)
        {
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    outputFormat = format;
                    return dt;
                }
            }

            outputFormat = null;
            return null;
        }
    }
}
