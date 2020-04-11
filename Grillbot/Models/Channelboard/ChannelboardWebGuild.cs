using Discord.WebSocket;

namespace Grillbot.Models.Channelboard
{
    public class ChannelboardWebGuild
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int UsersCount { get; set; }

        public static ChannelboardWebGuild Create(SocketGuild guild)
        {
            return new ChannelboardWebGuild()
            {
                AvatarUrl = guild.IconUrl,
                Name = guild.Name,
                UsersCount = guild.MemberCount
            };
        }
    }
}
