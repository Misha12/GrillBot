using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Models.Channelboard;
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
        public List<UserUnverifyHistoryItem> UnverifyHistory { get; set; }

        public long TotalMessageCount => Channels.Sum(o => o.Count);

        public DiscordUser(SocketGuild guild, SocketGuildUser user, DBDiscordUser dbUser, List<UserUnverifyHistoryItem> unverifyHistory)
        {
            Guild = guild;
            User = user;
            ID = dbUser.ID;
            Points = dbUser.Points;
            GivenReactionsCount = dbUser.GivenReactionsCount;
            ObtainedReactionsCount = dbUser.ObtainedReactionsCount;
            WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword);
            ApiAccess = !string.IsNullOrEmpty(dbUser.ApiToken);

            Channels = dbUser.Channels
                .Select(o => new ChannelStatItem(guild.GetChannel(o.ChannelIDSnowflake), o))
                .Where(o => o.Channel != null)
                .OrderByDescending(o => o.Count)
                .ToList();

            UnverifyHistory = unverifyHistory;
        }

        public string FormatReactions()
        {
            var given = FormatHelper.FormatWithSpaces(GivenReactionsCount);
            var obtained = FormatHelper.FormatWithSpaces(ObtainedReactionsCount);

            return $"{given} / {obtained}";
        }
    }
}
