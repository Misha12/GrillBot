using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Services.Preconditions;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Grillbot.Extensions.Discord;
using Grillbot.Extensions;
using Grillbot.Models.Config.AppSettings;
using Discord.Addons.Interactive;
using Grillbot.Models.Embed;

namespace Grillbot.Modules
{
    [Name("Nápověda")]
    [Group("grillhelp")]
    [Alias("help")]
    [RequirePermissions]
    public class HelpModule : BotModuleBase
    {
        private CommandService CommandService { get; }
        private IServiceProvider Services { get; }

        public HelpModule(CommandService commandService, IOptions<Configuration> config, IServiceProvider services) : base(config)
        {
            CommandService = commandService;
            Services = services;
        }

        [Command("")]
        public async Task HelpAsync()
        {
            var user = Context.Guild == null ? Context.User : Context.Guild.GetUser(Context.User.Id);

            var pages = new List<string>();
            foreach (var module in CommandService.Modules)
            {
                var descBuilder = new StringBuilder();

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, Services).ConfigureAwait(false);

                    if (result.IsSuccess)
                    {
                        descBuilder.Append(Config.CommandPrefix);

                        if (!string.IsNullOrEmpty(module.Group))
                            descBuilder.Append(module.Group).Append(' ');

                        descBuilder
                            .Append(cmd.Name).Append(' ')
                            .Append(string.Join(" ", cmd.Parameters.Select(o => "{" + o.Name + "}")));

                        if (!string.IsNullOrEmpty(cmd.Summary))
                            descBuilder.Append(" - ").AppendLine(cmd.Summary);
                        else
                            descBuilder.AppendLine();
                    }
                }

                if (descBuilder.Length > 0)
                {
                    descBuilder.Insert(0, $"**{module.Name}**:\n");
                    pages.Add(descBuilder.ToString());
                }
            }

            var paginated = new PaginatedMessage()
            {
                Color = Color.Blue,
                Pages = pages,
                Title = $"Nápověda pro uživatele {user.GetFullName()} ({GetBotBestPermissions(user)})",
                Options = new PaginatedAppearanceOptions()
                {
                    DisplayInformationIcon = false,
                    Stop = null
                }
            };

            await PagedReplyAsync(paginated);
        }

        [Command("")]
        public async Task HelpAsync([Remainder] string command)
        {
            var result = CommandService.Search(Context, command);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.UnknownCommand:
                        await ReplyAsync($"Je mi to líto, ale příkaz **{command.PreventMassTags()}** neznám.").ConfigureAwait(false);
                        break;
                    case CommandError.UnmetPrecondition:
                        await ReplyAsync(result.ErrorReason.PreventMassTags()).ConfigureAwait(false);
                        break;
                }

                return;
            }

            var embed = new BotEmbed(Context.User, title: $"Tady máš různé varianty příkazů na **{command.PreventMassTags()}**");

            foreach (var cmd in result.Commands.Select(o => o.Command))
            {
                var haveAccess = await cmd.CheckPreconditionsAsync(Context, Services).ConfigureAwait(false);

                if (haveAccess.IsSuccess)
                {
                    var valueBuilder = new StringBuilder();

                    if (cmd.Parameters.Count > 0)
                    {
                        valueBuilder
                            .Append("Parametry: ")
                            .AppendLine(string.Join(", ", cmd.Parameters.Select(p => p.Name)));
                    }

                    if (!string.IsNullOrEmpty(cmd.Summary))
                        valueBuilder.AppendLine(cmd.Summary);

                    if (!string.IsNullOrEmpty(cmd.Remarks))
                        valueBuilder.Append("Poznámka: ").AppendLine(cmd.Remarks.Replace("{prefix}", Config.CommandPrefix));

                    string commandDesc = valueBuilder.ToString();
                    embed.AddField(x =>
                    {
                        x.WithValue(string.IsNullOrEmpty(commandDesc) ? "Bez parametrů a popisu" : commandDesc)
                         .WithName(string.Join(", ", cmd.Aliases));
                    });
                }
            }

            if (embed.FieldsEmpty)
                embed.WithDescription($"Na metodu **{command.PreventMassTags()}** nemáš potřebná oprávnění.");

            await ReplyAsync(embed: embed.Build());
        }

        private string GetBotBestPermissions(SocketUser user)
        {
            if (Config.IsUserBotAdmin(user.Id))
                return "BotAdmin";

            if (user is SocketGuildUser sgUser)
            {
                return sgUser.Roles.FirstOrDefault(o => o.Position == sgUser.Roles.Max(x => x.Position))?.Name;
            }

            return "-";
        }
    }
}
