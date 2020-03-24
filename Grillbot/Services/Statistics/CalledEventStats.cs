using Discord.WebSocket;
using Grillbot.Database.Entity.Views;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Statistics
{
    public class CalledEventStats
    {
        public Dictionary<string, ulong> Data { get; }

        private LogRepository LogRepository { get; }
        private DiscordSocketClient Client { get; }

        public CalledEventStats(LogRepository logRepository, DiscordSocketClient client)
        {
            Data = new Dictionary<string, ulong>();
            LogRepository = logRepository;
            Client = client;
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

        public List<SummarizedCommandLog> GetSummarizedStats()
        {
            var data = LogRepository.GetSummarizedCommandLog();

            foreach (var item in data)
            {
                item.Methods = item.Methods.ToDictionary(o => o.Key, o =>
                {
                    var guild = Client.GetGuild(Convert.ToUInt64(o.Value));
                    return guild != null ? guild.Name : o.Value;
                });
            }

            return data;
        }
    }
}
