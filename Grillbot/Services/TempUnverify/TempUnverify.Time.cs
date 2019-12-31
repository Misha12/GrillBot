using System;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        /// <summary>
        /// Returns time for unverify in seconds;
        /// </summary>
        /// <param name="time"></param>
        private long ParseUnverifyTime(string time)
        {
            var timeWithoutSuffix = time.Substring(0, time.Length - 1);

            if (!timeWithoutSuffix.All(o => char.IsDigit(o)))
                throw new ArgumentException("Neplatný časový formát.");

            var convertedTime = Convert.ToInt64(timeWithoutSuffix);

            if (time.EndsWith("s"))
            {
                // Seconds
                if (convertedTime < 30)
                    throw new ArgumentException("Minimální čas pro unverify ve vteřinách je 30 sec");

                return convertedTime;
            }
            else if (time.EndsWith("m"))
            {
                // Minutes
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v minutách je 1 minuta.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromMinutes(convertedTime));
            }
            else if (time.EndsWith("h"))
            {
                // Hours
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify v hodinách je 1 hodina.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromHours(convertedTime));
            }
            else if (time.EndsWith("d"))
            {
                // Days
                if (convertedTime <= 0)
                    throw new ArgumentException("Minimální čas pro unverify ve dnech je 1 den.");

                return ConvertTimeSpanToSeconds(TimeSpan.FromDays(convertedTime));
            }
            else
            {
                throw new ArgumentException("Nepodporovaný časový formát.");
            }
        }

        private long ConvertTimeSpanToSeconds(TimeSpan timeSpan) => Convert.ToInt64(System.Math.Round(timeSpan.TotalSeconds));
    }
}
