using System.Collections.Generic;

namespace Grillbot.Services.Statistics
{
    public class InternalStatistics
    {
        public Dictionary<string, ulong> Events { get; }
        public Dictionary<string, ulong> Commands { get; }

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
    }
}
