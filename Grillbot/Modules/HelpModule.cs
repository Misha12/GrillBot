using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Preconditions;
using System.Collections.Generic;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;

namespace Grillbot.Modules
{
    [Group("grillhelp")]
    [Name("Nápověda")]
    [RequirePermissions("Help")]
    public class HelpModule : BotModuleBase
    {
        private CommandService CommandService { get; }
        private Configuration Config { get; }
        private IServiceProvider Services { get; }

        public HelpModule(CommandService commandService, IOptions<Configuration> config, IServiceProvider services)
        {
            CommandService = commandService;
            Config = config.Value;
            Services = services;
        }

        [Command("")]
        public async Task HelpAsync()
        {
            var user = Context.Guild == null ? Context.User : Context.Guild.GetUser(Context.User.Id);

            var embedFields = new List<EmbedFieldBuilder>();

            foreach(var module in CommandService.Modules)
            {
                var descBuilder = new StringBuilder();

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, Services);

                    if (result.IsSuccess)
                    {
                        descBuilder.Append(Config.CommandPrefix);

                        if (!string.IsNullOrEmpty(module.Group))
                            descBuilder.Append(module.Group).Append(' ');
                        
                        descBuilder
                            .Append(cmd.Name).Append(' ')
                            .Append(string.Join(" ", cmd.Parameters.Select(o => "{" + o.Name + "}")));

                        if(!string.IsNullOrEmpty(cmd.Summary))
                            descBuilder.Append(" - ").AppendLine(cmd.Summary);
                        else
                            descBuilder.AppendLine();
                    }
                }

                if (descBuilder.Length > 0)
                    embedFields.Add(new EmbedFieldBuilder().WithName(module.Name).WithValue(descBuilder.ToString()));
            }

            for(var i = 0; i < Math.Ceiling(embedFields.Count / (double)EmbedBuilder.MaxFieldCount); i++)
            {
                var fields = embedFields.Skip(i * EmbedBuilder.MaxFieldCount).Take(EmbedBuilder.MaxFieldCount).ToList();
                var embed = CreateEmbed(fields, user, i + 1);

                await ReplyAsync(embed: embed);
            }
        }

        private Embed CreateEmbed(List<EmbedFieldBuilder> fields, SocketUser user, int pageNumber)
        {
            var builder = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $"Nápověda pro uživatele {GetUsersFullName(user)} ({GetBotBestPermissions(user)})",
                Fields = fields,
                ThumbnailUrl = GetUserAvatarUrl(Context.Client.CurrentUser)
            };

            builder
                .WithFooter($"Odpověď pro {GetUsersShortName(user)} | Strana {pageNumber}", GetUserAvatarUrl(user))
                .WithCurrentTimestamp();

            return builder.Build();
        }

        [Command("")]
        public async Task HelpAsync([Remainder] string command)
        {
            var result = CommandService.Search(Context, command);

            if (!result.IsSuccess)
            {
                switch(result.Error)
                {
                    case CommandError.UnknownCommand:
                        await ReplyAsync($"Je mi to líto, ale příkaz **{command}** neznám.");
                        break;
                    case CommandError.UnmetPrecondition:
                        await ReplyAsync(result.ErrorReason);
                        break;
                }

                return;
            }

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $"Tady máš různé varianty příkazů na **{command}**"
            };

            foreach (var cmd in result.Commands.Select(o => o.Command))
            {
                var haveAccess = await cmd.CheckPreconditionsAsync(Context, Services);

                if (haveAccess.IsSuccess)
                {
                    var valueBuilder = new StringBuilder();

                    if (cmd.Parameters.Count > 0)
                    {
                        valueBuilder
                            .Append("Parametry: ")
                            .AppendLine(string.Join(", ", cmd.Parameters.Select(p => p.Name)));
                    }

                    if(!string.IsNullOrEmpty(cmd.Summary))
                        valueBuilder.AppendLine(cmd.Summary);

                    if (!string.IsNullOrEmpty(cmd.Remarks))
                        valueBuilder.Append("Poznámka: ").AppendLine(cmd.Remarks);

                    string commandDesc = valueBuilder.ToString();
                    embedBuilder.AddField(x =>
                    {
                        x.WithValue(string.IsNullOrEmpty(commandDesc) ? "Bez parametrů a popisu" : commandDesc)
                         .WithName(string.Join(", ", cmd.Aliases));
                    });
                }
            }

            if(embedBuilder.Fields.Count == 0)
                embedBuilder.Description = $"Na metodu **{command}** nemáš potřebná oprávnění";

            embedBuilder
                .WithCurrentTimestamp()
                .WithFooter($"Odpoveď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        private string GetBotBestPermissions(SocketUser user)
        {
            if (Config.Discord.IsUserBotAdmin(user.Id))
                return "BotAdmin";

            if (user is SocketGuildUser sgUser)
            {
                return sgUser.Roles.FirstOrDefault(o => o.Position == sgUser.Roles.Max(x => x.Position)).Name;
            }

            return "-";
        }
    }
}
