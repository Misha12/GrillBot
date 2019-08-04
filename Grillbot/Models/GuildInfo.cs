using Discord.WebSocket;

namespace Grillbot.Models
{
    public class GuildInfo
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int UsersCount { get; set; }

        public static GuildInfo Create(SocketGuild guild)
        {
            return new GuildInfo()
            {
                AvatarUrl = guild.IconUrl,
                Name = guild.Name,
                UsersCount = guild.Users.Count
            };
        }
    }
}
