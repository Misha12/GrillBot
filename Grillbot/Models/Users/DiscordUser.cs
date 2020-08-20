using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Math;
using Grillbot.Models.Unverify;
using Grillbot.Services.InviteTracker;
using System.Collections.Generic;
using System.Linq;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Models.Users
{
    public class DiscordUser
    {
        public long ID { get; set; }
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public long Points { get; set; }
        public long GivenReactionsCount { get; set; }
        public long ObtainedReactionsCount { get; set; }
        public bool WebAdminAccess { get; set; }
        public bool ApiAccess { get; set; }
        public List<ChannelStatItem> Channels { get; set; }
        public UserBirthday Birthday { get; set; }
        public List<MathAuditItem> MathAuditItems { get; set; }
        public long TotalMessageCount => Channels.Sum(o => o.Count);
        public StatisticItem Statistics { get; set; }
        public InviteModel UsedInvite { get; set; }
        public List<UnverifyLogItem> UnverifyHistory { get; set; }

        public DiscordUser(SocketGuild guild, SocketGuildUser user, DBDiscordUser dbUser, DiscordSocketClient discordClient)
        {
            Guild = guild;
            User = user;
            ID = dbUser.ID;
            Points = dbUser.Points;
            GivenReactionsCount = dbUser.GivenReactionsCount;
            ObtainedReactionsCount = dbUser.ObtainedReactionsCount;
            WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword);
            ApiAccess = !string.IsNullOrEmpty(dbUser.ApiToken);
            Birthday = dbUser.Birthday == null ? null : new UserBirthday(dbUser.Birthday);

            Channels = dbUser.Channels
                .Select(o => new ChannelStatItem(guild.GetChannel(o.ChannelIDSnowflake), o))
                .Where(o => o.Channel != null)
                .OrderByDescending(o => o.Count)
                .ToList();

            MathAuditItems = dbUser.MathAudit
                .OrderByDescending(o => o.ID)
                .Select(o => new MathAuditItem(o, guild))
                .ToList();

            if (dbUser.Statistics != null)
                Statistics = new StatisticItem(dbUser.Statistics);

            if (dbUser.UsedInvite != null)
            {
                SocketGuildUser inviteCreator = null;
                if (dbUser.UsedInvite.Creator != null)
                    inviteCreator = guild.GetUserFromGuildAsync(dbUser.UsedInvite.Creator.UserIDSnowflake).Result;

                UsedInvite = new InviteModel(dbUser.UsedInvite, inviteCreator);
            }

            if (dbUser.IncomingUnverifyOperations.Count > 0)
            {
                UnverifyHistory = dbUser.IncomingUnverifyOperations
                    .Where(o => o.Operation == UnverifyLogOperation.Unverify || o.Operation == UnverifyLogOperation.Selfunverify)
                    .Select(o => new UnverifyLogItem(o, discordClient)).ToList();
            }
        }

        public string FormatReactions()
        {
            var given = GivenReactionsCount.FormatWithSpaces();
            var obtained = ObtainedReactionsCount.FormatWithSpaces();

            return $"{given} / {obtained}";
        }

        public ChannelStatItem GetMostActiveChannel()
        {
            return Channels.FirstOrDefault();
        }

        public ChannelStatItem GetLastActiveChannel()
        {
            return Channels
                .OrderByDescending(o => o.LastMessageAt)
                .FirstOrDefault();
        }

        public List<string> GetDetailFlags()
        {
            var flags = new List<string>();

            if (WebAdminAccess)
                flags.Add("WebAdminAccess");

            if (ApiAccess)
                flags.Add("ApiAccess");

            if (User.IsGuildOwner())
                flags.Add("GuildOwner");

            if (Birthday != null)
                flags.Add("Birthday");

            return flags;
        }
    }
}
