using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Users
{
    public class WebAdminUser
    {
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public long Points { get; set; }
        public long GivenReactionsCount { get; set; }
        public long ObtainedReactionsCount { get; set; }
        public bool WebAdminAccess { get; set; }
        public string UserKey => $"{User.Id}|{Guild.Id}";

        public List<WebAdminUserChannel> Channels { get; set; }
        public long TotalMessageCount => Channels.Sum(o => o.Count);

        public WebAdminUser(SocketGuild guild, SocketGuildUser user, DiscordUser dbUser)
        {
            Guild = guild;
            User = user;

            Points = dbUser.Points;
            GivenReactionsCount = dbUser.GivenReactionsCount;
            ObtainedReactionsCount = dbUser.ObtainedReactionsCount;
            WebAdminAccess = !string.IsNullOrEmpty(dbUser.WebAdminPassword);

            Channels = dbUser.Channels.Select(o => new WebAdminUserChannel(guild.GetChannel(o.ChannelIDSnowflake), o)).ToList();
        }
    }
}
