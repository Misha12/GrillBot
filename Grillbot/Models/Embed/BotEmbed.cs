using Discord;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.Embed
{
    public class BotEmbed
    {
        private EmbedBuilder Builder { get; set; }

        public BotEmbed(IUser user, Color? color = null, string title = null)
        {
            Builder = new EmbedBuilder()
                .WithColor(color ?? Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {user.GetFullName()}", user.GetUserAvatarUrl());

            if (!string.IsNullOrEmpty(title))
                WithTitle(title);
        }

        public BotEmbed SetColor(Color color)
        {
            Builder = Builder.WithColor(color);
            return this;
        }

        public BotEmbed WithFields(params EmbedFieldBuilder[] fields)
        {
            Builder = Builder.WithFields(fields);
            return this;
        }

        public BotEmbed WithFields(IEnumerable<EmbedFieldBuilder> fields)
        {
            Builder = Builder.WithFields(fields);
            return this;
        }

        public BotEmbed WithTitle(string title)
        {
            Builder = Builder.WithTitle(title);
            return this;
        }

        public BotEmbed AddField(Action<EmbedFieldBuilder> action)
        {
            Builder.AddField(action);
            return this;
        }

        public Discord.Embed Build() => Builder.Build();
    }
}
