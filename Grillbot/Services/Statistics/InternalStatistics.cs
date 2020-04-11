using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Statistics
{
    public class InternalStatistics
    {
        private Dictionary<string, ulong> Events { get; }
        private Dictionary<string, ulong> Commands { get; }

        public InternalStatistics()
        {
            Events = new Dictionary<string, ulong>();
            Commands = new Dictionary<string, ulong>();
        }

        public void IncrementEvent(string name)
        {
            Increment(Events, name);
        }

        public void IncrementCommand(string name)
        {
            Increment(Commands, name);
        }

        private void Increment(Dictionary<string, ulong> counter, string name)
        {
            if (!counter.ContainsKey(name))
                counter.Add(name, 1);
            else
                counter[name]++;
        }

        public Dictionary<string, ulong> GetEvents()
        {
            return Events
                .OrderByDescending(o => o.Value)
                .ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Value);
        }

        public Dictionary<string, ulong> GetCommands()
        {
            return Commands
                .OrderByDescending(o => o.Value)
                .ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }
}
