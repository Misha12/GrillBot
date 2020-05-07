using Grillbot.Exceptions;
using System;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyTimeParser
    {
        /// <summary>
        /// Returns time for unverify in seconds;
        /// </summary>
        /// <param name="time">
        /// Supported time units: m, h, d
        /// </param>
        public int Parse(string time, int minimumMinutes = 30, int minimumHours = 1, int minimumDays = 1)
        {
            var timeWithoutSuffix = time[0..^1];
            var timeParseSuccess = int.TryParse(timeWithoutSuffix, out int convertedTime);

            if(!timeParseSuccess)
                throw new BotCommandInfoException("Neplatný časový formát.");

            if (time.EndsWith("m"))
            {
                ValidateTime(minimumMinutes, convertedTime, $"Minimální čas pro unverify v minutách je {minimumMinutes}.");
                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            
            if (time.EndsWith("h"))
            {
                ValidateTime(minimumHours, convertedTime, $"Minimální čas pro unverify v hodinách je {minimumHours}.");
                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            
            if (time.EndsWith("d"))
            {
                ValidateTime(minimumDays, convertedTime, $"Minimální čas pro unverify ve dnech je {minimumDays}.");
                return ConvertTimeSpanToSeconds(TimeSpan.FromDays(convertedTime));
            }

            throw new BotCommandInfoException("Nepodporovaný časový formát.");
        }

        private int ConvertTimeSpanToSeconds(TimeSpan timeSpan)
        {
            var totalSecs = timeSpan.TotalSeconds;

            if (totalSecs * 1000 >= int.MaxValue)
                throw new ArgumentException("Maximální čas pro unverify je 24 dní (576 hodin).");

            return Convert.ToInt32(System.Math.Round(timeSpan.TotalSeconds));
        }

        private void ValidateTime(int minimum, int value, string message)
        {
            if (value < minimum)
                throw new BotCommandInfoException(message);
        }
    }
}
