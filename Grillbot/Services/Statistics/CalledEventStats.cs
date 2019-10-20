using Grillbot.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Statistics
{
    public class CalledEventStats
    {
        public Dictionary<string, ulong> Data { get; }

        public CalledEventStats()
        {
            Data = new Dictionary<string, ulong>();
        }

        public void Increment(string eventName)
        {
            if (!Data.ContainsKey(eventName))
                Data.Add(eventName, 1);
            else
                Data[eventName]++;
        }

        public Dictionary<string, string> GetValues()
        {
            return Data
                .OrderByDescending(o => o.Value)
                .ThenByDescending(o => o.Key)
                .ToDictionary(o => o.Key, o => FormatHelper.FormatWithSpaces(o.Value));
        }
    }
}
