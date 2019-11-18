using Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models
{
    public class TopStack
    {
        public List<Tuple<DateTime, string, string>> Data { get; }
        private int Capacity { get; }
        private static object Locker { get; } = new object();

        public TopStack() : this(5)
        {
        }

        public TopStack(int capacity)
        {
            Data = new List<Tuple<DateTime, string, string>>();
            Capacity = capacity;
        }

        public void Add(IMessage message, string info = null)
        {
            lock(Locker)
            {
                if (message == null)
                    return;

                if (Data.Count == Capacity)
                    Data.RemoveAt(0);

                Data.Add(new Tuple<DateTime, string, string>(DateTime.Now, info, message.GetJumpUrl()));
            }
        }

        public void Clear() => Data.Clear();
    }
}
