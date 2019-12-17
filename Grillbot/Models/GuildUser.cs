using Discord;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using System;
using System.Linq;

namespace Grillbot.Models
{
    public class GuildUser
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }
        public GuildUserStatus Status { get; set; } = GuildUserStatus.Other;

        public static GuildUser Create(SocketGuildUser user)
        {
            var guildUser = new GuildUser()
            {
                AvatarUrl = user.GetUserAvatarUrl(),
                Discriminator = user.Discriminator,
                Name = user.Username,
                Nickname = user.Nickname
            };

            if (user.Activity?.Type == ActivityType.Listening && string.Equals(user.Activity.Name, "spotify", StringComparison.InvariantCultureIgnoreCase))
                guildUser.Status = GuildUserStatus.Spotify;
            else if ((new[] { UserStatus.AFK, UserStatus.Idle }).Contains(user.Status))
                guildUser.Status = GuildUserStatus.Idle;
            else if (user.Status == UserStatus.DoNotDisturb)
                guildUser.Status = GuildUserStatus.DoNotDisturb;
            else if (user.Status == UserStatus.Online)
                guildUser.Status = GuildUserStatus.Online;

            return guildUser;
        }
    }
}