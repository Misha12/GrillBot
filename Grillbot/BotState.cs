using Discord;
using Grillbot.Services.InviteTracker;
using System;
using System.Collections.Generic;

namespace Grillbot
{
    public class BotState
    {
        // Key: $"{guild.Id}|{user.Id}|{limit}"
        // Value is datetime of last calculation.
        public Dictionary<string, DateTime> LastPointsCalculation { get; set; }

        // Key: Guild ID
        // Value: List of invites for guild.
        public Dictionary<ulong, List<InviteModel>> InviteCache { get; set; }

        // Key: $"{guild.Id}|{user.Id}"
        // Value is end datetime of unverify.
        public Dictionary<string, DateTime> UnverifyCache { get; set; }

        public List<IUser> CurrentReturningUnverifyFor { get; set; }

        public BotState()
        {
            LastPointsCalculation = new Dictionary<string, DateTime>();
            InviteCache = new Dictionary<ulong, List<InviteModel>>();
            UnverifyCache = new Dictionary<string, DateTime>();
            CurrentReturningUnverifyFor = new List<IUser>();
        }
    }
}
