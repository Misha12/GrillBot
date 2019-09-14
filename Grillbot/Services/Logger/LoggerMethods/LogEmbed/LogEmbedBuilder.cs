using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger.LoggerMethods.LogEmbed
{
    public class LogEmbedBuilder
    {
        private string Header { get; set; }
        private string Title { get; set; }
        private LogEmbedType Type { get; }

        private EmbedFooterBuilder FooterBuilder { get; set; }
        private bool CanSetTimestamp { get; set; }
        private List<EmbedFieldBuilder> FieldBuilders { get; }
        private string AvatarUrl { get; set; }
        private string ImageUrl { get; set; }

        public LogEmbedBuilder(string title, LogEmbedType type)
        {
            Header = title;
            Type = type;

            FieldBuilders = new List<EmbedFieldBuilder>();
        }

        public Embed Build()
        {
            var builder = new EmbedBuilder()
            {
                Color = GetColor(Type),
                Author = new EmbedAuthorBuilder().WithName(Header),
                ThumbnailUrl = AvatarUrl,
                Title = Title
            };

            if (CanSetTimestamp)
                builder.WithCurrentTimestamp();

            if (FooterBuilder != null)
                builder.WithFooter(FooterBuilder);

            if (!string.IsNullOrEmpty(ImageUrl))
                builder.WithImageUrl(ImageUrl);

            builder.WithFields(FieldBuilders);
            return builder.Build();
        }

        public LogEmbedBuilder SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public LogEmbedBuilder SetAuthor(IUser user)
        {
            AddField("Uživatel", user?.ToString() ?? "Neznámý");
            AvatarUrl = user?.GetAvatarUrl() ?? user?.GetDefaultAvatarUrl();

            return this;
        }

        public LogEmbedBuilder SetFooter(string text)
        {
            FooterBuilder = new EmbedFooterBuilder();
            FooterBuilder.WithText(text);

            return this;
        }

        public LogEmbedBuilder SetFooter(ulong messageId)
        {
            FooterBuilder = new EmbedFooterBuilder();
            FooterBuilder.WithText($"MessageID: {messageId}");

            return this;
        }

        public LogEmbedBuilder SetTimestamp(bool active)
        {
            CanSetTimestamp = active;
            return this;
        }

        public LogEmbedBuilder AddField(string name, object value, bool isInline = false)
        {
            var field = new EmbedFieldBuilder();

            field
                .WithName(name)
                .WithValue(CutToDiscordLimit(value.ToString()))
                .WithIsInline(isInline);

            FieldBuilders.Add(field);
            return this;
        }

        public LogEmbedBuilder AddCodeBlockField(string name, string value, bool isInline = false)
        {
            var formated = FormatData(value);

            AddField(name, $"```{formated}```", isInline);
            return this;
        }

        private string CutToDiscordLimit(string content)
        {
            const int embedSize = 1018;

            if (content.Length > embedSize)
                return content.Substring(0, embedSize - 3) + "...";

            return content;
        }

        private string FormatData(object input)
        {
            if (!(input is string str))
                return input.ToString();

            var formated = str.Replace("```", "``");

            if (formated.EndsWith("`"))
                formated += " ";

            return formated;
        }

        public LogEmbedBuilder SetImage(string url)
        {
            ImageUrl = url;
            return this;
        }

        private Color GetColor(LogEmbedType type)
        {
            switch(type)
            {
                case LogEmbedType.MessageDeleted: return Color.Red;
                case LogEmbedType.MessageEdited: return new Color(255, 255, 0);
                case LogEmbedType.UserJoined:
                case LogEmbedType.UserLeft:
                case LogEmbedType.UserUpdated:
                    return Color.Green;
                case LogEmbedType.GuildMemberUpdated: return Color.DarkBlue;
                default: return Color.Blue;
            }
        }
    }
}
