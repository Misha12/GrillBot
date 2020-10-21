using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Grillbot.Services.Statistics.ApiStats
{
    public class ApiStatistics
    {
        public List<ApiStatsData> Data { get; }

        public ApiStatistics()
        {
            Data = new List<ApiStatsData>()
            {
                new ApiStatsData(new Regex(@"GET\s*guilds\/\d*\/invites"), "Get invites"),
                new ApiStatsData(new Regex(@"GET\s*channels\/\d*\/messages(\?.*)?"), "Get messages"),
                new ApiStatsData(new Regex(@"POST\s*channels\/\d*\/messages"), "Send message"),
                new ApiStatsData(new Regex(@"GET\s*guilds\/\d*\/vanity-url"), "Get vanity URL"),
                new ApiStatsData(new Regex(@"GET\s*invites\/(.*)(\?.*)?"), "Get invite"),
                new ApiStatsData(new Regex(@"GET\s*guilds\/\d*\/invites"), "Get invites of guild"),
                new ApiStatsData(new Regex(@"GET\s*guilds\/\d*\/audit-logs(\?.*)"), "Get audit logs"),
                new ApiStatsData(new Regex(@"PUT\s*guilds\/\d*\/members\/\d*\/roles\/\d*"), "Add guild member role"),
                new ApiStatsData(new Regex(@"DELETE\s*guilds\/\d*\/members\/\d*\/roles\/\d*"), "Remove guild member role"),
                new ApiStatsData(new Regex(@"PUT\s*channels\/\d*\/permissions\/\d*"), "Edit channel permissions"),
                new ApiStatsData(new Regex(@"POST\s*users\/@me\/channels"), "Create DM"),
                new ApiStatsData(new Regex(@"PUT\s*channels\/\d*\/messages\/\d*\/reactions\/(.*)\/(@me|\d*)"), "Create reaction"),
                new ApiStatsData(new Regex(@"PATCH\s*channels\/\d*\/messages\/\d*"), "Edit message"),
                new ApiStatsData(new Regex(@"DELETE\s*channels\/\d*\/messages\/\d*\/reactions\/(.*)\/(@me|\d*)"), "Delete reaction"),
                new ApiStatsData(null, "OtherAPICalls")
            };
        }

        public void Increment(LogMessage message)
        {
            if (!string.Equals(message.Source, "rest", StringComparison.InvariantCultureIgnoreCase)) return;

            var requestMatch = Regex.Match(message.Message, @"([GET|HEAD|POST|PUT|DELETE|CONNECT|OPTIONS|TRACE|PATCH]\s*.*?):\s*([\d,\.]*)\s*ms");

            if (!requestMatch.Success)
                return;

            var endpoint = requestMatch.Groups[1].Value;
            var time = requestMatch.Groups[2].Value;

            bool isMatch = false;
            foreach (var item in Data.Where(o => o.ParseRegex != null))
            {
                if (!item.ParseRegex.IsMatch(endpoint))
                    continue;

                isMatch = true;
                ProcessMatch(item, time);
            }

            if (!isMatch)
            {
                var otherCallsItem = Data.Find(o => o.MethodName == "OtherAPICalls");
                ProcessMatch(otherCallsItem, time);
            }
        }

        private void ProcessMatch(ApiStatsData data, string time)
        {
            data.Count++;

            var withDot = double.TryParse(time.Replace(",", "."), out double res) ? res : -1.0;
            var withoutDot = double.TryParse(time, out res) ? res : -1.0;
            var selectedTime = withDot > -1.0 ? withDot : withoutDot;
            if (selectedTime == -1) return;

            var timespan = TimeSpan.FromMilliseconds(selectedTime);

            if (timespan > data.MaxTime)
                data.MaxTime = timespan;

            if (timespan < data.MinTime)
                data.MinTime = timespan;

            data.AvgTime = data.AvgTime == TimeSpan.Zero ? timespan : (data.AvgTime + timespan) / 2.0;
            data.TotalTime += timespan;
        }
    }
}
