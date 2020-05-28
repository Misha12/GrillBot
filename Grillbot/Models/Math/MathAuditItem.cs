using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Math;
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

        public MathAuditItem(MathAuditLogItem item, SocketGuild guild)
        {
            ID = item.ID;
            Expression = item.Expression;
            DateTime = item.DateTime;
            Channel = guild.GetTextChannel(item.ChannelIDSnowflake);

            var unitInfo = JsonConvert.DeserializeObject<MathUnitInfo>(item.UnitInfo);
            UnitInfo = $"#{unitInfo.SessionID} {(unitInfo.ForBooster ? "(Booster)" : "")} ({unitInfo.ComputeLimit / 1000.0}sec)";

            var result = JsonConvert.DeserializeObject<MathCalcResult>(item.Result);
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
    }
}
