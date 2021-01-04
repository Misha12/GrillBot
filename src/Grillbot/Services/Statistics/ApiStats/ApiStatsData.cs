using System;
using System.Text.RegularExpressions;

namespace Grillbot.Services.Statistics.ApiStats
{
    public class ApiStatsData
    {
        public Regex ParseRegex { get; set; }
        public string MethodName { get; set; }
        public ulong Count { get; set; }

        public TimeSpan MinTime { get; set; }
        public TimeSpan MaxTime { get; set; }
        public TimeSpan AvgTime { get; set; }
        public TimeSpan TotalTime { get; set; }

        public ApiStatsData(Regex parseRegex, string name)
        {
            ParseRegex = parseRegex;
            MethodName = name;

            MinTime = TimeSpan.MaxValue;
            MaxTime = TimeSpan.MinValue;
            AvgTime = TimeSpan.Zero;
            TotalTime = TimeSpan.Zero;
        }
    }
}
