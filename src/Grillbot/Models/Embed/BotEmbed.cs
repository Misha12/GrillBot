﻿using Discord;
using Grillbot.Extensions.Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Embed
{
    public class BotEmbed
    {
        private EmbedBuilder Builder { get; }

        public bool FieldsEmpty
        {
            get => Builder.Fields.Count == 0;
        }

        public BotEmbed(IUser responseFor = null, Color? color = null, string title = null, string thumbnail = null)
        {
            Builder = new EmbedBuilder()
                .WithColor(color ?? Color.Blue)
                .WithCurrentTimestamp();

            if (responseFor != null)
                Builder.WithFooter($"Odpověď pro {responseFor.GetFullName()}", responseFor.GetUserAvatarUrl());

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
            foreach (var field in fields.Where(o => o != null))
            {
                AddField(field.Name, field.Value?.ToString(), field.IsInline);
            }

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
            var field = new EmbedFieldBuilder();
            action(field);

            AddField(field);
            return this;
        }

        public BotEmbed AddField(EmbedFieldBuilder field)
        {
            AddField(field.Name, field.Value?.ToString(), field.IsInline);
            return this;
        }

        public BotEmbed AddField(string name, string value, bool inline)
        {
            if (name.Length >= EmbedFieldBuilder.MaxFieldNameLength)
                name = name.Substring(0, EmbedFieldBuilder.MaxFieldNameLength - 3) + "...";

            if (value == null)
                throw new ArgumentException($"Neplatná hodnota NULL v objektu s klíčem {name}.");

            if (value.Length >= EmbedFieldBuilder.MaxFieldValueLength)
            {
                if (value.StartsWith("```") && value.EndsWith("```"))
                {
                    // Cut CodeBlocks.

                    var tmp = value
                        .Substring(3)
                        .Substring(0, value.Length - 3)
                        .Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 9);

                    value = "```" + tmp + "```";
                }
                else if (value.StartsWith("`") && value.EndsWith("`"))
                {
                    // CUT `...` blocks

                    var tmp = value
                        .Substring(1)
                        .Substring(0, value.Length - 1)
                        .Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 5);

                    value = "`" + tmp + "...`";
                }
                else
                {
                    value = value.Substring(0, EmbedFieldBuilder.MaxFieldValueLength - 3) + "...";
                }
            }

            Builder.AddField(o => o.WithName(name).WithValue(value).WithIsInline(inline));
            return this;
        }

        public BotEmbed WithThumbnail(string url)
        {
            Builder.WithThumbnailUrl(url);
            return this;
        }

        public Discord.Embed Build()
        {
            return Builder.Build();
        }

        public EmbedBuilder GetBuilder()
        {
            return Builder;
        }

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

        public BotEmbed WithFooter(string footer, string iconUrl)
        {
            Builder.WithFooter(o => o.WithText(footer).WithIconUrl(iconUrl));
            return this;
        }

        public BotEmbed ClearFields()
        {
            Builder.Fields.Clear();
            return this;
        }

        public BotEmbed WithDescription(string description)
        {
            Builder.WithDescription(description);
            return this;
        }
    }
}
