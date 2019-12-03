using Discord;
using System.Collections.Generic;

namespace Grillbot.Models
{
    public class EmoteMergeListItem
    {
        public Dictionary<string, long> Emotes { get; set; }
        public string MergeTo => Emote.ToString();
        public Emote Emote { get; set; }

        public EmoteMergeListItem()
        {
            Emotes = new Dictionary<string, long>();
        }
    }
}
