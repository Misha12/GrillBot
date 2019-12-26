using Discord;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;

namespace Grillbot.Models.Embed
{
    public class BotEmbed
    {
        private EmbedBuilder Builder { get; }

        public BotEmbed(IUser user, Color? color = null, string title = null, string thumbnail = null)
        {
            Builder = new EmbedBuilder()
                .WithColor(color ?? Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {user.GetFullName()}", user.GetUserAvatarUrl());

            if (!string.IsNullOrEmpty(title))
                WithTitle(title);

            if (!string.IsNullOrEmpty(thumbnail))
                WithThumbnail(thumbnail);
        }

        public BotEmbed SetColor(Color color)
        {
            Builder.WithColor(color);
            return this;
        }

        public BotEmbed WithFields(params EmbedFieldBuilder[] fields)
        {
            Builder.WithFields(fields);
            return this;
        }

        public BotEmbed WithFields(IEnumerable<EmbedFieldBuilder> fields)
        {
            Builder.WithFields(fields);
            return this;
        }

        public BotEmbed WithTitle(string title)
        {
            Builder.WithTitle(title);
            return this;
        }

        public BotEmbed AddField(Action<EmbedFieldBuilder> action)
        {
            Builder.AddField(action);
            return this;
        }

        public BotEmbed WithThumbnail(string url)
        {
            Builder.WithThumbnailUrl(url);
            return this;
        }

        public Discord.Embed Build() => Builder.Build();

        public BotEmbed PrependFooter(string footer)
        {
            Builder.WithFooter(string.Join(" | ", new[] { footer, Builder.Footer.Text }), Builder.Footer.IconUrl);
            return this;
        }

        public BotEmbed WithAuthor(string name, string iconUrl = null, string url = null)
        {
            Builder.WithAuthor(name, iconUrl, url);
            return this;
        }

        public BotEmbed WithAuthor(IUser user)
        {
            Builder.WithAuthor(user);
            return this;
        }
    }
}
