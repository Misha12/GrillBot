using Discord;
using Discord.WebSocket;
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
                AvatarUrl = user.GetAvatarUrl(ImageFormat.Png),
                Discriminator = user.Discriminator,
                Name = user.Username,
                Nickname = user.Nickname
            };

            if (user.Activity != null && user.Activity.Type == ActivityType.Listening && user.Activity.Name.ToLower() == "spotify")
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