using Discord;
using Discord.WebSocket;
using Grillbot.Enums;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.Unverify
{
    public class UnverifyLeaderboardViewModel
    {
        public SocketGuild Guild { get; set; }
        public LeaderboardErrors Error { get; set; }

        public Dictionary<IUser, Tuple<int, int>> Stats { get; set; }

        public UnverifyLeaderboardViewModel(SocketGuild guild, Dictionary<IUser, Tuple<int, int>> stats) : this(LeaderboardErrors.Success)
        {
            Guild = guild;
            Stats = stats ?? new Dictionary<IUser, Tuple<int, int>>();
        }

        public UnverifyLeaderboardViewModel(LeaderboardErrors error)
        {
            Error = error;
        }
    }
}
