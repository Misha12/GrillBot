using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Channelboard;
using Grillbot.Models.Unverify;
using Grillbot.Services.InviteTracker;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public long TotalMessageCount => Channels.Sum(o => o.Count);
        public StatisticItem Statistics { get; set; }
        public InviteModel UsedInvite { get; set; }
        public List<UnverifyLogItem> UnverifyHistory { get; set; }
        public long Flags { get; set; }

        #region FlagFields

        public bool IsBotAdmin => (Flags & (long)UserFlags.BotAdmin) != 0;

        #endregion

        public static async Task<DiscordUser> CreateAsync(SocketGuild guild, SocketGuildUser user, DBDiscordUser dbUser, DiscordSocketClient discordClient,
            RestApplication botAppInfo)
        {
            var result = new DiscordUser()
            {
                ApiAccess = !string.IsNullOrEmpty(dbUser.ApiToken),
                Guild = guild,
                User = user,
                ID = dbUser.ID,
                Points = dbUser.Points,
                GivenReactionsCount = dbUser.GivenReactionsCount,
                ObtainedReactionsCount = dbUser.ObtainedReactionsCount,
                WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword),
                Birthday = dbUser.Birthday == null ? null : new UserBirthday(dbUser.Birthday),
                Statistics = dbUser.Statistics == null ? null : new StatisticItem(dbUser.Statistics),
                Flags = dbUser.Flags
            };

            if (botAppInfo.Owner.Id == user.Id)
                result.Flags |= (long)UserFlags.BotAdmin;

            result.Channels = dbUser.Channels
                .Select(o => new ChannelStatItem(guild.GetChannel(o.ChannelIDSnowflake), o))
                .Where(o => o.Channel != null)
                .OrderByDescending(o => o.Count)
                .ToList();

            if (dbUser.UsedInvite != null)
            {
                SocketGuildUser inviteCreator = null;
                if (dbUser.UsedInvite.Creator != null)
                    inviteCreator = await guild.GetUserFromGuildAsync(dbUser.UsedInvite.Creator.UserIDSnowflake);

                result.UsedInvite = new InviteModel(dbUser.UsedInvite, inviteCreator);
            }
            else if(!string.IsNullOrEmpty(dbUser.UsedInviteCode))
            {
                result.UsedInvite = new InviteModel(dbUser.UsedInviteCode);
            }

            result.UnverifyHistory = dbUser.IncomingUnverifyOperations
                    .Where(o => o.Operation == UnverifyLogOperation.Unverify || o.Operation == UnverifyLogOperation.Selfunverify)
                    .Select(o => new UnverifyLogItem(o, discordClient))
                    .ToList();

            return result;
        }

        public DiscordUser() { }

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

            if (IsBotAdmin)
                flags.Add("BotAdmin");

            return flags;
        }
    }
}
