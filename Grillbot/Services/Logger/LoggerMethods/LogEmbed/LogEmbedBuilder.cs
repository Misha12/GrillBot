using Discord;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
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
                .WithCurrentTimestamp()
                .WithColor(Type.GetColor())
                .WithAuthor(Header)
                .WithThumbnailUrl(AvatarUrl)
                .WithTitle(Title);

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

        public LogEmbedBuilder SetAuthor(IUser user, bool strictFormatAsAuthor = false)
        {
            AddField(strictFormatAsAuthor ? "Autor" : "Uživatel", user?.ToString() ?? "Neznámý");
            AvatarUrl = user?.GetUserAvatarUrl();

            return this;
        }

        public LogEmbedBuilder SetFooter(string text)
        {
            FooterBuilder = new EmbedFooterBuilder();
            FooterBuilder.WithText(text);

            return this;
        }

        public LogEmbedBuilder AddField(string name, object value, bool isInline = false, bool cut = true)
        {
            var field = new EmbedFieldBuilder();

            field
                .WithName(name)
                .WithValue(cut ? CutToDiscordLimit(value.ToString()) : value.ToString())
                .WithIsInline(isInline);

            FieldBuilders.Add(field);
            return this;
        }

        public LogEmbedBuilder AddCodeBlockField(string name, string value, bool isInline = false)
        {
            var formated = FormatData(value);

            AddField(name, $"```{CutToDiscordLimit(formated)}```", isInline, false);
            return this;
        }

        private string CutToDiscordLimit(string content)
        {
            const int embedSize = 1018;

            if (content.Length > embedSize)
                return content.Substring(0, embedSize - 3) + "...";

            return content;
        }

        private string FormatData(string input)
        {
            if (input == null)
                input = "";

            var formated = input.Replace("```", "``");

            if (formated.EndsWith("`"))
                formated += " ";

            return formated;
        }

        public LogEmbedBuilder SetImage(string url)
        {
            ImageUrl = url;
            return this;
        }
    }
}
