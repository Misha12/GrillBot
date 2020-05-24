using Discord.WebSocket;
using Grillbot.Database.Entity.UnverifyLog;
using Grillbot.Extensions.Discord;
using System;

namespace Grillbot.Models.Users
{
    public class UserUnverifyHistoryItem
    {
        public long AuditID { get; set; }
        public bool IsSelfUnverify { get; set; }
        public TimeSpan Time { get; set; }
        public string FromUser { get; set; }
        public string Reason { get; set; }

        public UserUnverifyHistoryItem(UnverifyLog logItem)
        {
            var data = logItem.Json.ToObject<UnverifyLogSet>();

            AuditID = logItem.ID;
            IsSelfUnverify = data.IsSelfUnverify;
            Time = TimeSpan.FromSeconds(Convert.ToInt32(data.TimeFor));
            Reason = data.Reason;
        }
    }
}
