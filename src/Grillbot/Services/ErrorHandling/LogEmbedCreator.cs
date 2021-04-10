using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Models.Embed;
using System;

namespace Grillbot.Services.ErrorHandling
{
    public class LogEmbedCreator
    {
        private DiscordSocketClient DiscordClient { get; }

        public LogEmbedCreator(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
        }

        public BotEmbed CreateErrorEmbed(LogMessage message, ErrorLogItem logItem)
        {
            if (message.Exception is CommandException commandException)
            {
                return CreateCommandErrorEmbed(commandException, logItem);
            }

            return CreateGenericErrorEmbed(logItem, message.Source);
        }

        private BotEmbed CreateCommandErrorEmbed(CommandException exception, ErrorLogItem logItem)
        {
            return new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Při provádění příkazu došlo k chybě")
                .AddField("ID záznamu", logItem.ID.ToString(), true)
                .AddField("Kanál", $"<#{exception.Context.Channel.Id}>", true)
                .AddField("Uživatel", exception.Context.User.Mention, true)
                .AddField("Zpráva", $"```{exception.Context.Message.Content}```", false)
                .AddField("Skok na zprávu", exception.Context.Message.GetJumpUrl(), false);
        }

        private BotEmbed CreateGenericErrorEmbed(ErrorLogItem logItem, string source)
        {
            return new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Došlo k neočekávané chybě.")
                .AddField("ID záznamu", logItem.ID.ToString(), true)
                .AddField("Zdroj", source, true);
        }
    }
}
