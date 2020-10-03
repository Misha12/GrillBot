using Grillbot.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Grillbot.Services.Unverify
{
    public class UnverifyTimeParser
    {
        private Regex ParseRegex { get; }

        public UnverifyTimeParser()
        {
            ParseRegex = new Regex("^(\\d+)(m|h|d|M|y)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Parse time of unverify and returns DateTime of unvereify end.
        /// </summary>
        public DateTime Parse(string time, int minimumMinutes = 30)
        {
            if (DateTime.TryParse(time, out DateTime dateTime))
                return ValidateAndReturnDateTime(dateTime, minimumMinutes);

            var groups = ParseRegex.Match(time);

            if (!groups.Success)
                throw new ValidationException("Neplatný, nebo nepodporovaný časový formát.");

            var result = DateTime.Now;
            var timeValue = Convert.ToInt32(groups.Groups[1].Value);

            switch (groups.Groups[2].Value)
            {
                case "m": // minutes
                    result = result.AddMinutes(timeValue);
                    break;
                case "h": // hours
                    result = result.AddHours(timeValue);
                    break;
                case "d": // days
                    result = result.AddDays(timeValue);
                    break;
                case "M": // Months
                    result = result.AddMonths(timeValue);
                    break;
                case "y": // Years
                    result = result.AddYears(timeValue);
                    break;
            }

            return ValidateAndReturnDateTime(result, minimumMinutes);
        }

        private DateTime ValidateAndReturnDateTime(DateTime dateTime, int minimumMinutes)
        {
            var diff = dateTime - DateTime.Now;

            if (diff.TotalMinutes < 0)
                throw new ValidationException("Konec unverify musí být v budoucnosti.");

            if (System.Math.Round(diff.TotalMinutes) < minimumMinutes)
                throw new ValidationException($"Minimální čas pro unverify je {TimeSpan.FromMinutes(minimumMinutes).ToCzechLongTimeString()}");

            return dateTime;
        }
    }
}
