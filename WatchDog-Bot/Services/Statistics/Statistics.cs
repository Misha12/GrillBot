using System.Collections.Generic;
using System.Linq;

namespace WatchDog_Bot.Services.Statistics
{
    public class Statistics
    {
        public Dictionary<string, StatisticsData> Data { get; }

        public Statistics()
        {
            Data = new Dictionary<string, StatisticsData>();
        }

        public void LogCall(string command, long elapsedTime)
        {
            if (!Data.ContainsKey(command))
                Data.Add(command, new StatisticsData(command, elapsedTime));
            else
                Data[command].Increment(elapsedTime);
        }

        public List<StatisticsData> GetOrderedData(bool byTime)
        {
            if (byTime)
                return Data.Values.OrderByDescending(o => o.AverageTime).ToList();

            return Data.Values.OrderByDescending(o => o.CallsCount).ToList();
        }
    }
}
