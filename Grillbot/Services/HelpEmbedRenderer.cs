using Discord;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Embed;
using Grillbot.Models.PaginatedEmbed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class HelpEmbedRenderer
    {
        private CommandService CommandService { get; }
        private IServiceProvider ServiceProvider { get; }
        private string CommandPrefix { get; }

        public HelpEmbedRenderer(CommandService commandService, IServiceProvider serviceProvider, IOptions<Configuration> options)
        {
            CommandService = commandService;
            ServiceProvider = serviceProvider;
            CommandPrefix = options.Value.CommandPrefix;
        }

        public async Task<PaginatedEmbed> RenderSummaryHelpAsync(SocketCommandContext context)
        {
            var pages = new List<PaginatedEmbedPage>();

            foreach (var module in CommandService.Modules)
            {
                var page = await CreatePageAsync(module, context);

                if (page.AnyField())
                    pages.Add(page);
            }

            return new PaginatedEmbed()
            {
                Title = "Nápověda",
                Pages = pages,
                ResponseFor = context.User,
                Thumbnail = context.Client.CurrentUser.GetUserAvatarUrl()
            };
        }

        private async Task<PaginatedEmbedPage> CreatePageAsync(ModuleInfo module, SocketCommandContext context)
        {
            var page = new PaginatedEmbedPage($"**{module.Name}**");

            foreach (var command in module.Commands)
            {
                var result = await command.CheckPreconditionsAsync(context, ServiceProvider);

                if (result.IsSuccess)
                    AddCommandToPage(page, module, command);
            }

            return page;
        }

        private void AddCommandToPage(PaginatedEmbedPage page, ModuleInfo module, CommandInfo command)
        {
            var builder = new StringBuilder()
                .Append(CommandPrefix);

            if (!string.IsNullOrEmpty(module.Group))
                builder.Append(module.Group).Append(' ');

            builder
                .Append(command.Name).Append(' ')
                .Append(string.Join(" ", command.Parameters.Select(o => "{" + o.Name + "}")));

            var field = new EmbedFieldBuilder()
                .WithName(builder.ToString())
                .WithValue(ProcessSummary(command));

            page.AddField(field);
        }

        private string ProcessSummary(CommandInfo command)
        {
            if (string.IsNullOrEmpty(command.Summary))
                return "-";

            return (command.Summary.ToLower()) switch
            {
                "<frommodule(name)>" => command.Module.Name,
                _ => command.Summary,
            };
        }

        public async Task<BotEmbed> RenderCommandHelpAsync(string command, SocketCommandContext context)
        {
            var result = CommandService.Search(command);

            if (!result.IsSuccess)
                ProcessSearchError(result, command);

            var embed = new BotEmbed(context.User, title: $"Tady máš různé varianty příkazů na **{command.PreventMassTags()}**");

            foreach (var cmd in result.Commands.Select(o => o.Command))
            {
                var access = await cmd.CheckPreconditionsAsync(context, ServiceProvider);

                if (!access.IsSuccess)
                    continue;

                embed.AddField(CreateCommandDetail(cmd));
            }

            if (embed.FieldsEmpty)
                embed.WithDescription($"Na metodu **{command.PreventMassTags()}** nemáš žádná potřebná oprávnění.");

            return embed;
        }

        private EmbedFieldBuilder CreateCommandDetail(CommandInfo command)
        {
            var builder = new StringBuilder();

            if(command.Parameters.Count > 0)
            {
                builder
                    .Append("Parametry: ")
                    .AppendLine(string.Join(", ", command.Parameters.Select(o => o.Name)));
            }

            if (!string.IsNullOrEmpty(command.Summary))
                builder.AppendLine(command.Summary);

            if(!string.IsNullOrEmpty(command.Remarks))
                builder.Append("Poznámka: ").AppendLine(command.Remarks.Replace("{prefix}", CommandPrefix));

            return new EmbedFieldBuilder()
                .WithName(string.Join(", ", command.Aliases))
                .WithValue(builder.Length == 0 ? "Bez parametrů a popisu" : builder.ToString());
        }

        private void ProcessSearchError(SearchResult result, string command)
        {
            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    throw new NotFoundException($"Je mi to líto, ale příkaz **{command.PreventMassTags()}** neznám.");
                case CommandError.UnmetPrecondition:
                    throw new UnauthorizedAccessException(result.ErrorReason.PreventMassTags());
            }
        }
    }
}
