using Discord;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;

namespace Grillbot.Services.Logger.LoggerMethods.LogEmbed
{
    public class LogEmbedBuilder
    {
        private BotEmbed Embed { get; }

        public LogEmbedBuilder(string title, LogEmbedType type)
        {
            Embed = new BotEmbed(title: title, color: type.GetColor());
        }

        public Embed Build() => Embed.Build();

        public LogEmbedBuilder SetTitle(string title)
        {
            Embed.WithTitle(title);
            return this;
        }

        public LogEmbedBuilder SetAuthor(IUser user, bool strictFormatAsAuthor = false)
        {
            AddField(strictFormatAsAuthor ? "Autor" : "Uživatel", user?.ToString() ?? "Neznámý");
            Embed.WithThumbnail(user?.GetUserAvatarUrl());

            return this;
        }

        public LogEmbedBuilder SetFooter(string text)
        {
            Embed.WithFooter(text, null);
            return this;
        }

        public LogEmbedBuilder AddField(string name, object value, bool isInline = false, bool cut = true)
        {
            Embed.AddField(name, value.ToString(), isInline);
            return this;
        }

        public LogEmbedBuilder AddCodeBlockField(string name, string value, bool isInline = false)
        {
            var formated = FormatData(value);

            AddField(name, $"```{formated}```", isInline, false);
            return this;
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
    }
}
