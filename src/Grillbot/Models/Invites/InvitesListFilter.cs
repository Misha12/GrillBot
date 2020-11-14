using Discord.WebSocket;
using System;
using System.Linq;

namespace Grillbot.Models.Invites
{
    public class InvitesListFilter
    {
        public ulong GuildID { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string UserQuery { get; set; }
        public bool Desc { get; set; } = true;

        public static InvitesListFilter CreateDefault(DiscordSocketClient client)
        {
            return new InvitesListFilter()
            {
                GuildID = client.Guilds.First().Id
            };
        }
    }
}
