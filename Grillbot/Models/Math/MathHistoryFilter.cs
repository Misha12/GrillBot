using Discord;
using Discord.Commands;
using System;

namespace Grillbot.Models.Math
{
    [NamedArgumentType]
    public class MathHistoryFilter
    {
        public IUser User { get; set; }
        public IChannel Channel { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
    }
}
