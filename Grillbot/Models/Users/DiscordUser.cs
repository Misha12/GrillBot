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
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public long Points { get; set; }
        public long GivenReactionsCount { get; set; }
        public long ObtainedReactionsCount { get; set; }
        public bool WebAdminAccess { get; set; }
        public string UserKey => $"{User.Id}|{Guild.Id}";

        public List<ChannelStatItem> Channels { get; set; }
        public long TotalMessageCount => Channels.Sum(o => o.Count);

        public DiscordUser(SocketGuild guild, SocketGuildUser user, DBDiscordUser dbUser)
        {
            Guild = guild;
            User = user;

            Points = dbUser.Points;
            GivenReactionsCount = dbUser.GivenReactionsCount;
            ObtainedReactionsCount = dbUser.ObtainedReactionsCount;
            WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword);

            Channels = dbUser.Channels
                .Select(o => new ChannelStatItem(guild.GetChannel(o.ChannelIDSnowflake), o))
                .Where(o => o.Channel != null)
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        public string FormatReactions()
        {
            var given = FormatHelper.FormatWithSpaces(GivenReactionsCount);
            var obtained = FormatHelper.FormatWithSpaces(ObtainedReactionsCount);

            return $"{given} / {obtained}";
        }
    }
}
