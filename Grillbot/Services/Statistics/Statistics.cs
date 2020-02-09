using Grillbot.Models;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Statistics
{
    public class Statistics
    {
        public Dictionary<string, StatisticsData> Data { get; }
        public double AvgReactTime { get; private set; }
        private Configuration Config { get; }

        public Statistics(IOptions<Configuration> configuration)
        {
            Data = new Dictionary<string, StatisticsData>();
            Config = configuration.Value;
        }

        public void LogCall(string command, long elapsedTime)
        {
            if (command.StartsWith(Config.CommandPrefix))
                command = command.Substring(1);

            if (!Data.ContainsKey(command))
                Data.Add(command, new StatisticsData(command, elapsedTime));
            else
                Data[command].Increment(elapsedTime);
        }

        public List<StatisticsData> GetOrderedData()
        {
            return Data.Values.OrderByDescending(o => o.CallsCount).ToList();
        }

        public void ComputeAvgReact(long elapsedTime)
        {
            AvgReactTime = (AvgReactTime + elapsedTime) / 2.0D;
        }

        public TimeSpan GetAvgReactTime() => TimeSpan.FromMilliseconds(AvgReactTime);
    }
}