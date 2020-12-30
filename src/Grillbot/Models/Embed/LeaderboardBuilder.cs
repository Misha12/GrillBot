using Discord;
using Grillbot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Embed
{
    /// <summary>
    /// Leaderboard embed model.
    /// </summary>
    public class LeaderboardBuilder
    {
        public Dictionary<string, string> Data { get; set; }
        public string Title { get; set; }
        public IUser User { get; set; }
        public Color? Color { get; set; }
        public string Thumbnail { get; set; }
        public int Skip { get; set; }

        public LeaderboardBuilder(string title, IUser user, string thumbnail = null, Color? color = null)
        {
            Data = new Dictionary<string, string>();

            Title = title;
            User = user;
            Color = color;
            Thumbnail = thumbnail;
        }

        public void AddItem(string key, string value)
        {
            Data[key] = value;
        }

        public void SetData(Dictionary<string, string> data)
        {
            Data.Clear();
            Data.AddRange(data);
        }

        public Discord.Embed Build()
        {
            var botEmbed = new BotEmbed(User, Color, Title, Thumbnail);

            if (Data.Count == 0)
                botEmbed.WithDescription("V tomto žebříčku nejsou žádná data.");
            else
                botEmbed.WithDescription(string.Join(Environment.NewLine, Data.Select((o, i) => $"> {Skip + i + 1}: {o.Key}: {o.Value}")));

            return botEmbed.Build();
        }
    }
}
