using Discord.Rest;
using Grillbot.Database.Entity;
using Grillbot.Services.InviteTracker;
using System;
using System.Collections.Concurrent;
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

        public RestApplication AppInfo { get; set; }

        // Key: $"{guild.Id}|{user.Id}"
        // Value: DB ID
        public ConcurrentDictionary<string, long> UserToID { get; set; }

        public List<AutoReplyItem> AutoReplyItems { get; set; }

        public BotState()
        {
            LastPointsCalculation = new Dictionary<string, DateTime>();
            InviteCache = new Dictionary<ulong, List<InviteModel>>();
            UserToID = new ConcurrentDictionary<string, long>();
            AutoReplyItems = new List<AutoReplyItem>();
        }
    }
}
