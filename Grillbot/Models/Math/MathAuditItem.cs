using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Math;
using Grillbot.Extensions.Discord;
using Newtonsoft.Json;
using System;

namespace Grillbot.Models.Math
{
    public class MathAuditItem
    {
        public int ID { get; set; }
        public string Expression { get; set; }
        public DateTime DateTime { get; set; }
        public IChannel Channel { get; set; }
        public string UnitInfo { get; set; }
        public string Result { get; set; }
        public IUser User { get; set; }

        public MathAuditItem(MathAuditLogItem item, SocketGuild guild)
        {
            ID = item.ID;
            Expression = item.Expression;
            DateTime = item.DateTime;
            Channel = guild.GetTextChannel(item.ChannelIDSnowflake);
            User = guild.GetUserFromGuildAsync(item.User.UserIDSnowflake).Result;

            var unitInfo = JsonConvert.DeserializeObject<MathUnitInfo>(item.UnitInfo);
            UnitInfo = $"#{unitInfo.SessionID} {(unitInfo.ForBooster ? "(Booster)" : "")} ({unitInfo.ComputeLimit / 1000.0}sec)".Trim();

            var result = JsonConvert.DeserializeObject<MathCalcResult>(item.Result);

            if (result == null)
                result = new MathCalcResult() { ErrorMessage = "Výpočetní jednotka nevrátila data." };

            if (result.IsValid)
            {
                Result = $"{result.Result} ({result.GetComputingTime()})";
            }
            else
            {
                if (result.IsTimeout)
                    Result = $"Timeout ({result.GetComputingTime()})";
                else
                    Result = $"Error ({result.ErrorMessage}) ({result.GetComputingTime()})";
            }
        }

        public MathAuditItem(MathAuditLogItem item, DiscordSocketClient client) : this(item, client.GetGuild(item.User.GuildIDSnowflake))
        {
        }
    }
}
