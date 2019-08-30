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

        private EmbedAuthorBuilder AuthorBuilder { get; set; }
        private EmbedFooterBuilder FooterBuilder { get; set; }
        private bool CanSetTimestamp { get; set; }
        private List<EmbedFieldBuilder> FieldBuilders { get; }
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
                Title = Title,
                Color = GetColor(Type),
                Author = AuthorBuilder
            };

            if (CanSetTimestamp)
                builder.WithCurrentTimestamp();

            if (FooterBuilder != null)
                builder.WithFooter(FooterBuilder);

            builder.WithFields(FieldBuilders);
            return builder.Build();
        }

        public LogEmbedBuilder SetAuthor(IUser user)
        {
            AuthorBuilder = new EmbedAuthorBuilder();

            if (user == null)
                AuthorBuilder.WithName("Neznámý uživatel");
            else
                AuthorBuilder.WithIconUrl(user.GetAvatarUrl()).WithName(user.Username);

            return this;
        }

        public LogEmbedBuilder SetFooter(IMessage message)
        {
            CanSetTimestamp = true;
            FooterBuilder = new EmbedFooterBuilder();

            FooterBuilder
                .WithText($"MessageID: {message.Id} | AuthorID: {message.Author?.Id}");

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
