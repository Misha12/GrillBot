using System;
using WatchDog_Bot.Helpers;

namespace WatchDog_Bot.Services.Statistics
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
    }
}
