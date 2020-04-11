using System;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        /// <summary>
        /// Returns time for unverify in seconds;
        /// </summary>
        /// <param name="time">
        /// Supported time units: m, h, d
        /// </param>
        private int ParseUnverifyTime(string time, int minimumMinutes = 30, int minimumHours = 1, int minimumDays = 1)
        {
            var timeWithoutSuffix = time[0..^1];

            if (!timeWithoutSuffix.All(o => char.IsDigit(o)))
                throw new ArgumentException("Neplatný časový formát.");

            int convertedTime;
            try
            {
                convertedTime = Convert.ToInt32(timeWithoutSuffix);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Neplatný časový formát.");
            }

            if (time.EndsWith("m"))
            {
                // Minutes
                if (convertedTime < minimumMinutes)
                    throw new ArgumentException($"Minimální čas pro unverify v minutách je {minimumMinutes}.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            else if (time.EndsWith("h"))
            {
                // Hours
                if (convertedTime <= minimumHours)
                    throw new ArgumentException($"Minimální čas pro unverify v hodinách je {minimumHours}.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            else if (time.EndsWith("d"))
            {
                // Days
                if (convertedTime <= minimumDays)
                    throw new ArgumentException($"Minimální čas pro unverify ve dnech je {minimumDays}.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromDays(convertedTime));
            }
            else
            {
                throw new ArgumentException("Nepodporovaný časový formát.");
            }
        }

        private int ConvertTimeSpanToSeconds(TimeSpan timeSpan)
        {
            var totalSecs = timeSpan.TotalSeconds;

            if (totalSecs * 1000 >= int.MaxValue)
                throw new ArgumentException("Maximální čas pro unverify je 24 dní (576 hodin).");

            return Convert.ToInt32(System.Math.Round(timeSpan.TotalSeconds));
        }
    }
}
