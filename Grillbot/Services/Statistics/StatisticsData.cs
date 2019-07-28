using System;
#pragma warning disable CS0234 // The type or namespace name 'Helpers' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)
using Grillbot.Helpers;
#pragma warning restore CS0234 // The type or namespace name 'Helpers' does not exist in the namespace 'Grillbot' (are you missing an assembly reference?)

namespace Grillbot.Services.Statistics
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
