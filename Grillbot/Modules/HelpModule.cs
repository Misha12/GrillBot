using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Services;

namespace Grillbot.Modules
{
    [Name("Nápověda")]
#pragma warning disable CS0246 // The type or namespace name 'BotModuleBase' could not be found (are you missing a using directive or an assembly reference?)
    public class HelpModule : BotModuleBase
#pragma warning restore CS0246 // The type or namespace name 'BotModuleBase' could not be found (are you missing a using directive or an assembly reference?)
    {
        private CommandService CommandService { get; }
        private string CommandPrefix { get; }
        private IServiceProvider Services { get; }

        public HelpModule(CommandService commandService, IConfiguration config, IServiceProvider services)
        {
            CommandService = commandService;
            CommandPrefix = config["CommandPrefix"];
            Services = services;
        }

        [Command("grilhelp")]
        [RequireRole(RoleGroupName = "Help")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder() { Color = new Color(114, 137, 218) };

            foreach(var module in CommandService.Modules)
            {
                var descBuilder = new StringBuilder();

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, Services);

                    if (result.IsSuccess)
                    {
                        descBuilder
                            .Append(CommandPrefix)
                            .Append(cmd.Name).Append(' ')
                            .Append(string.Join(" ", cmd.Parameters.Select(o => "{" + o.Name + "}")));

                        if(!string.IsNullOrEmpty(cmd.Summary))
                            descBuilder.Append(" - ").AppendLine(cmd.Summary);
                        else
                            descBuilder.AppendLine();
                    }
                }

                if (descBuilder.Length > 0)
                {
                    embed.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = descBuilder.ToString();
                    });
                }
            }

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("grilhelp")]
        [RequireRole(RoleGroupName = "Help")]
        public async Task HelpAsync(string command)
        {
            var result = CommandService.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Je mi to líto, ale příkaz **{command}** neznám.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Tady máš různé varianty příkazů na **{command}**"
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
                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = string.IsNullOrEmpty(commandDesc) ? "Bez parametrů a popisu" : commandDesc;
                    });
                }
            }

            if(builder.Fields.Count == 0)
            {
                builder.Description = $"Na metodu **{command}** nemáš potřebná oprávnění";
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
