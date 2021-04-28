using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Models.Embed;

namespace Grillbot.Services.ErrorHandling
{
    public class LogEmbedCreator
    {
        private DiscordSocketClient DiscordClient { get; }

        public LogEmbedCreator(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
        }

        public BotEmbed CreateErrorEmbed(LogMessage message)
        {
            if (message.Exception is CommandException commandException)
            {
                return CreateCommandErrorEmbed(commandException);
            }

            return CreateGenericErrorEmbed(message.Source);
        }

        private BotEmbed CreateCommandErrorEmbed(CommandException exception)
        {
            return new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Při provádění příkazu došlo k chybě")
                .AddField("Kanál", $"<#{exception.Context.Channel.Id}>", true)
                .AddField("Uživatel", exception.Context.User.Mention, true)
                .AddField("Zpráva", $"```{exception.Context.Message.Content}```", false)
                .AddField("Skok na zprávu", exception.Context.Message.GetJumpUrl(), false);
        }

        private BotEmbed CreateGenericErrorEmbed(string source)
        {
            return new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Došlo k neočekávané chybě.")
                .AddField("Zdroj", source, true);
        }
    }
}
