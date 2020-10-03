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

        public BotEmbed CreateErrorEmbed(LogMessage message, ErrorLogItem logItem, string backupFilename = null)
        {
            if (message.Exception is CommandException commandException)
            {
                return CreateCommandErrorEmbed(commandException, logItem, backupFilename);
            }

            return CreateGenericErrorEmbed(message.Exception, logItem, backupFilename);
        }

        private BotEmbed CreateCommandErrorEmbed(CommandException exception, ErrorLogItem logItem, string backupFilename)
        {
            var embed = new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Při provádění příkazu došlo k chybě");

            if (!string.IsNullOrEmpty(backupFilename))
                embed.AddField("Záložní soubor", backupFilename, true);
            else
                embed.AddField("ID záznamu", logItem.ID.ToString(), true);

            embed
                .AddField("Kanál", $"<#{exception.Context.Channel.Id}>", true)
                .AddField("Uživatel", exception.Context.User.Mention, true)
                .AddField("Zpráva", $"```{exception.Context.Message.Content}```", false)
                .AddField(exception.InnerException.GetType().Name, $"```{exception}```", false);

            return embed;
        }

        private BotEmbed CreateGenericErrorEmbed(Exception exception, ErrorLogItem logItem, string backupFilename)
        {
            var embed = new BotEmbed(DiscordClient.CurrentUser, Color.Red, "Došlo k neočekávané chybě.");

            if (!string.IsNullOrEmpty(backupFilename))
                embed.AddField("Záložní soubor", backupFilename, false);
            else
                embed.AddField("ID záznamu", logItem.ID.ToString(), false);

            embed
                .AddField(exception.GetType().Name, $"```{exception}```", false);

            return embed;
        }
    }
}
