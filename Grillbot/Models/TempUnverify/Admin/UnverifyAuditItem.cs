using Discord.WebSocket;
using Grillbot.Database.Entity.UnverifyLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

            switch (log.Operation)
            {
                case UnverifyLogOperation.AutoRemove:
                case UnverifyLogOperation.Remove:
                    {
                        var data = log.Json.ToObject<UnverifyLogRemove>();
                        RemoveLogData = new AuditItemRemoveOperation()
                        {
                            OverridedChannels = data.Overrides.Select(o => Guild.GetChannel(o.ChannelIdSnowflake)).Where(o => o != null).ToList(),
                            Roles = data.Roles.Select(o => Guild.GetRole(o)).ToList()
                        };
                    }
                    break;
                case UnverifyLogOperation.Set:
                    {
                        var data = log.Json.ToObject<UnverifyLogSet>();
                        SetLogData = new AuditItemSetOperation()
                        {
                            OverridedChannels = data.Overrides.Select(o => Guild.GetChannel(o.ChannelIdSnowflake)).Where(o => o != null).ToList(),
                            Roles = data.Roles.Select(o => Guild.GetRole(o)).ToList(),
                            Reason = data.Reason,
                            StartAt = data.StartAt,
                            Time = data.TimeFor,
                            IsSelfUnverify = data.IsSelfUnverify,
                            Subjects = data.Subjects ?? new List<string>()
                        };
                    }
                    break;
                case UnverifyLogOperation.Update:
                    UpdateLogData = new AuditItemUpdateOperation() { Time = log.Json.ToObject<UnverifyLogUpdate>().TimeFor };
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

        public bool IsSelfUnverify()
        {
            return Operation == UnverifyLogOperation.Set && SetLogData.IsSelfUnverify;
        }
    }
}
