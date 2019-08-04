using Discord.WebSocket;

namespace Grillbot.Models
{
    public class GuildUser
    {
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }

        public static GuildUser Create(SocketGuildUser user)
        {
            return new GuildUser()
            {
                AvatarUrl = user.GetAvatarUrl(Discord.ImageFormat.Png),
                Discriminator = user.Discriminator,
                Name = user.Username,
                Nickname = user.Nickname
            };
        }
    }
}