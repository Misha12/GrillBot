using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Logger.LoggerMethods.LogEmbed
{
    public class LogEmbedBuilder
    {
        private string Title { get; set; }
        private LogEmbedType Type { get; }

        private EmbedFooterBuilder FooterBuilder { get; set; }
        private bool CanSetTimestamp { get; set; }
        private List<EmbedFieldBuilder> FieldBuilders { get; }
        private string AvatarUrl { get; set; }
        private string ImageUrl { get; set; }

        public LogEmbedBuilder(string title, LogEmbedType type)
        {
            Title = title;
            Type = type;

            FieldBuilders = new List<EmbedFieldBuilder>();
        }

        public Embed Build()
        {
            var builder = new EmbedBuilder()
            {
                Color = GetColor(Type),
                Author = new EmbedAuthorBuilder().WithName(Title),
                ThumbnailUrl = AvatarUrl
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

        public LogEmbedBuilder SetAuthor(IUser user)
        {
            AddField("Uživatel", user?.ToString() ?? "Neznámý");
            AvatarUrl = user?.GetAvatarUrl() ?? user?.GetDefaultAvatarUrl();

            return this;
        }

        public LogEmbedBuilder SetFooter(IMessage message)
        {
            FooterBuilder = new EmbedFooterBuilder();
            FooterBuilder.WithText($"MessageID: {message.Id} | AuthorID: {message.Author?.Id}");

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
                .WithValue(value)
                .WithIsInline(isInline);

            FieldBuilders.Add(field);
            return this;
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
                case LogEmbedType.MessageDeleted:
                    return Color.Red;

                default:
                    return Color.Blue;
            }
        }
    }
}
