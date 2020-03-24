using Grillbot.Database.Entity.Views;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Statistics
{
    public class CalledEventStats
    {
        public Dictionary<string, ulong> Data { get; }

        private LogRepository LogRepository { get; }

        public CalledEventStats(LogRepository logRepository)
        {
            Data = new Dictionary<string, ulong>();
            LogRepository = logRepository;
        }

        public void Increment(string eventName)
        {
            if (!Data.ContainsKey(eventName))
                Data.Add(eventName, 1);
            else
                Data[eventName]++;
        }

        public Dictionary<string, string> ToFormatedDictionary()
        {
            return Data
                .OrderByDescending(o => o.Value)
                .ThenByDescending(o => o.Key)
                .ToDictionary(o => o.Key, o => FormatHelper.FormatWithSpaces(o.Value));
        }

        public List<SummarizedCommandLog> GetSummarizedStats() => LogRepository.GetSummarizedCommandLog();
    }
}
