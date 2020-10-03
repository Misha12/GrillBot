using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Unverify.Models.Log
{
    public class UnverifyLogSet : UnverifyLogBase
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public List<ulong> RolesToKeep { get; set; }
        public List<ulong> RolesToRemove { get; set; }
        public List<ChannelOverwrite> ChannelsToKeep { get; set; }
        public List<ChannelOverwrite> ChannelsToRemove { get; set; }
        public string Reason { get; set; }

        public static UnverifyLogSet FromProfile(UnverifyUserProfile profile)
        {
            return new UnverifyLogSet()
            {
                ChannelsToKeep = profile.ChannelsToKeep,
                ChannelsToRemove = profile.ChannelsToRemove,
                EndDateTime = profile.EndDateTime,
                Reason = profile.Reason,
                RolesToKeep = profile.RolesToKeep.Select(o => o.Id).ToList(),
                RolesToRemove = profile.RolesToRemove.Select(o => o.Id).ToList(),
                StartDateTime = profile.StartDateTime
            };
        }
    }
}
