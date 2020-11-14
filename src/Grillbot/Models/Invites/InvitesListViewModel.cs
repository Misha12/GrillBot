using Discord.WebSocket;
using Grillbot.Services.InviteTracker;
using System.Collections.Generic;

namespace Grillbot.Models.Invites
{
    public class InvitesListViewModel
    {
        public List<SocketGuild> Guilds { get; }
        public List<InviteModel> Invites { get; }
        public InvitesListFilter Filter { get; }

        public InvitesListViewModel(List<SocketGuild> guilds, List<InviteModel> invites, InvitesListFilter filter)
        {
            Guilds = guilds;
            Invites = invites;
            Filter = filter;
        }
    }
}
