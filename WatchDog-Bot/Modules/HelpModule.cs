using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog_Bot.Modules
{
    [Name("Help")]
    public class HelpModule : BotModuleBase
    {
        private CommandService CommandService { get; }
        private string CommandPrefix { get; }

        public HelpModule(CommandService commandService, IConfigurationRoot config)
        {
            CommandService = commandService;
            CommandPrefix = config["CommandPrefix"];
        }

        [Command("doghelp")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder() { Color = new Color(114, 137, 218) };

            foreach(var module in CommandService.Modules)
            {
                var descBuilder = new StringBuilder();

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);

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

        [Command("doghelp")]
        public async Task HelpAsync(string command)
        {
            var result = CommandService.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var cmd in result.Commands.Select(o => o.Command))
            {
                var valueBuilder = new StringBuilder();

                if(cmd.Parameters.Count > 0)
                {
                    valueBuilder
                        .Append("Parameters: ")
                        .AppendLine(string.Join(", ", cmd.Parameters.Select(p => p.Name)));
                }

                valueBuilder.Append("Summary: ").AppendLine(cmd.Summary);

                if(!string.IsNullOrEmpty(cmd.Remarks))
                    valueBuilder.Append("Remarks: ").AppendLine(cmd.Remarks);

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = valueBuilder.ToString();
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
