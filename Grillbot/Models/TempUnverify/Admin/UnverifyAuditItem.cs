using Discord.WebSocket;
using Grillbot.Database.Entity.UnverifyLog;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyAuditItem
    {
        public UnverifyAuditItem(UnverifyLog log, DiscordSocketClient client)
        {
            ID = log.ID;
            Operation = log.Operation;
            DateTime = log.DateTime;
            Guild = client.GetGuild(log.GuildIDSnowflake);

            switch(log.Operation)
            {
                case UnverifyLogOperation.AutoRemove:
                case UnverifyLogOperation.Remove:
                    var removeData = JsonConvert.DeserializeObject<UnverifyLogRemove>(log.Data);
                    RemoveLogData = new AuditItemRemoveOperation()
                    {
                        OverridedChannels = removeData.Overrides.Select(o => Guild.GetChannel(o.ChannelIdSnowflake)).Where(o => o != null).ToList(),
                        Roles = removeData.Roles.Select(o => Guild.GetRole(o)).ToList()
                    };
                    break;
                case UnverifyLogOperation.Set:
                    var setData = JsonConvert.DeserializeObject<UnverifyLogSet>(log.Data);
                    SetLogData = new AuditItemSetOperation()
                    {
                        OverridedChannels = setData.Overrides.Select(o => Guild.GetChannel(o.ChannelIdSnowflake)).Where(o => o != null).ToList(),
                        Roles = setData.Roles.Select(o => Guild.GetRole(o)).ToList(),
                        Reason = setData.Reason,
                        StartAt = setData.StartAt,
                        Time = setData.TimeFor
                    };
                    break;
                case UnverifyLogOperation.Update:
                    var updateData = JsonConvert.DeserializeObject<UnverifyLogUpdate>(log.Data);
                    UpdateLogData = new AuditItemUpdateOperation() { Time = updateData.TimeFor };
                    break;
            }
        }

        public int ID { get; set; }
        public UnverifyLogOperation Operation { get; set; }
        public SocketUser FromUser { get; set; }
        public SocketUser ToUser { get; set; }
        public SocketGuild Guild { get; set; }
        public DateTime DateTime { get; set; }

        #region LogData

        public AuditItemRemoveOperation RemoveLogData { get; set; }
        public AuditItemSetOperation SetLogData { get; set; }
        public AuditItemUpdateOperation UpdateLogData { get; set; }

        #endregion
    }
}
