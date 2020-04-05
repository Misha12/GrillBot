using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Grillbot.Database.Entity.Views
{
    public class SummarizedCommandLog
    {
        public string Group { get; set; }
        public string Command { get; set; }
        public int Count { get; set; }
        public Dictionary<int, string> Methods { get; set; }
        public int PermissionsCount { get; set; }

        public void ReplaceGuildNames(DiscordSocketClient client)
        {
            var methods = new Dictionary<int, string>();

            foreach(var method in Methods)
            {
                var guild = client.GetGuild(Convert.ToUInt64(method.Value));

                if (guild != null)
                    methods.Add(method.Key, guild.Name);
                else
                    methods.Add(method.Key, method.Value);
            }

            Methods = methods;
        }
    }
}
