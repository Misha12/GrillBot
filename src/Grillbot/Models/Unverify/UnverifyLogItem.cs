using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using System;
using System.Linq;

namespace Grillbot.Models.Unverify
{
    public class UnverifyLogItem
    {
        public long ID { get; set; }
        public UnverifyLogOperation Operation { get; set; }
        public IUser FromUser { get; set; }
        public IUser ToUser { get; set; }
        public DateTime DateTime { get; set; }

        #region DeserializedData

        public UnverifyUserProfile Profile { get; set; }
        public UnverifyLogUpdate UpdateData { get; set; }
        public UnverifyRemoveOperation RemoveData { get; set; }

        #endregion

        public UnverifyLogItem(UnverifyLog entity, DiscordSocketClient client)
        {
            var guild = client.GetGuild(entity.FromUser.GuildIDSnowflake);

            ID = entity.ID;
            Operation = entity.Operation;
            DateTime = entity.CreatedAt;
            FromUser = guild.GetUserFromGuildAsync(entity.FromUser.UserIDSnowflake).Result;
            ToUser = guild.GetUserFromGuildAsync(entity.ToUser.UserIDSnowflake).Result;

            switch (entity.Operation)
            {
                case UnverifyLogOperation.Autoremove:
                case UnverifyLogOperation.Remove:
                case UnverifyLogOperation.Recover:
                    var removeLogData = entity.Json.ToObject<UnverifyLogRemove>();
                    RemoveData = new UnverifyRemoveOperation()
                    {
                        ReturnedChannels = removeLogData.ReturnedOverrides.Select(o => guild.GetChannel(o.ChannelID)).Where(o => o != null).ToList(),
                        ReturnedRoles = removeLogData.ReturnedRoles.Select(o => guild.GetRole(o)).Where(o => o != null).ToList()
                    };
                    break;
                case UnverifyLogOperation.Selfunverify:
                case UnverifyLogOperation.Unverify:
                    var logData = entity.Json.ToObject<UnverifyLogSet>();
                    Profile = new UnverifyUserProfile()
                    {
                        EndDateTime = logData.EndDateTime,
                        Reason = logData.Reason,
                        StartDateTime = logData.StartDateTime,
                        ChannelsToKeep = logData.ChannelsToKeep.Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelID), new OverwritePermissions(o.AllowValue, o.DenyValue))).Where(o => o.Channel != null).ToList(),
                        ChannelsToRemove = logData.ChannelsToRemove.Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelID), new OverwritePermissions(o.AllowValue, o.DenyValue))).Where(o => o.Channel != null).ToList(),
                        RolesToKeep = logData.RolesToKeep.Select(o => guild.GetRole(o)).Where(o => o != null).ToList(),
                        RolesToRemove = logData.RolesToRemove.Select(o => guild.GetRole(o)).Where(o => o != null).ToList()
                    };
                    break;
                case UnverifyLogOperation.Update:
                    UpdateData = entity.Json.ToObject<UnverifyLogUpdate>();
                    break;
            }
        }
    }
}
