using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Grillbot.Services.Unverify.Models
{
    public class UnverifyUserProfile
    {
        public IUser DestinationUser { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public List<SocketRole> RolesToRemove { get; set; }
        public List<SocketRole> RolesToKeep { get; set; }
        public List<ChannelOverwrite> ChannelsToRemove { get; set; }
        public List<ChannelOverwrite> ChannelsToKeep { get; set; }
        public string Reason { get; set; }
        public bool IsSelfUnverify { get; set; }

        public UnverifyUserProfile()
        {
            RolesToRemove = new List<SocketRole>();
            RolesToKeep = new List<SocketRole>();
            ChannelsToRemove = new List<ChannelOverwrite>();
            ChannelsToKeep = new List<ChannelOverwrite>();
        }
    }
}
