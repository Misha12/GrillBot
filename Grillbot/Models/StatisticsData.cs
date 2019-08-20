using System;
using Grillbot.Helpers;

namespace Grillbot.Models
{
    public class StatisticsData
    {
        public string Command { get; set; }
        public uint CallsCount { get; set; }
        public double AverageTime { get; set; }

        public StatisticsData(string command, long elapsed)
        {
            Command = command;
            Increment(elapsed);
        }

        public void Increment(long elapsedTime)
        {
            CallsCount++;
            AverageTime = (AverageTime + elapsedTime) / 2.0D;
        }

        public override string ToString()
        {
            return $"{Command}\t{FormatHelper.FormatWithSpaces(CallsCount)}\t{TimeSpan.FromMilliseconds(AverageTime)}";
        }

        public TimeSpan GetAverageTime() => TimeSpan.FromMilliseconds(AverageTime);
    }
}
