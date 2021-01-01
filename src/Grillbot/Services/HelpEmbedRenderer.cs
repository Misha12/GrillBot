using Discord;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services.Config;
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

        public HelpEmbedRenderer(CommandService commandService, IServiceProvider serviceProvider, ConfigurationService configurationService)
        {
            CommandService = commandService;
            ServiceProvider = serviceProvider;

            CommandPrefix = configurationService.GetValue(Enums.GlobalConfigItems.CommandPrefix);
            if (string.IsNullOrEmpty(CommandPrefix))
                CommandPrefix = "$";
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
                return "_ _";

            return (command.Summary.ToLower()) switch
            {
                "<frommodule(name)>" => command.Module.Name,
                "<frommodule(remarks)" => command.Module.Remarks,
                _ => command.Summary,
            };
        }

        public async Task<BotEmbed> RenderCommandHelpAsync(string command, SocketCommandContext context)
        {
            var module = CommandService.Modules.FirstOrDefault(o => o.Group == command || o.Aliases.Contains(command));

            if (module == null)
            {
                var searchResult = CommandService.Search(command);

                if (!searchResult.IsSuccess)
                    ProcessSearchError(searchResult, command);

                return await RenderEmbedAsync(command, searchResult.Commands.Select(o => o.Command), context, searchResult.Commands[0].Command.Module.Remarks);
            }

            return await RenderEmbedAsync(command, module, context);
        }

        private async Task<BotEmbed> RenderEmbedAsync(string prefix, ModuleInfo module, ICommandContext context)
        {
            return await RenderEmbedAsync(prefix, module.Commands, context, module.Remarks);
        }

        private async Task<BotEmbed> RenderEmbedAsync(string prefix, IEnumerable<CommandInfo> commands, ICommandContext context, string remarks)
        {
            const string baseMessage = "Tady máš různé varianty příkazů na \"**{0}**\"";
            var embed = new BotEmbed(context.User, title: string.Format(baseMessage, prefix.Cut(EmbedBuilder.MaxTitleLength - baseMessage.Length - 5)));

            foreach (var cmd in commands)
            {
                var access = await cmd.CheckPreconditionsAsync(context, ServiceProvider);

                if (!access.IsSuccess)
                    continue;

                embed.AddField(CreateCommandDetail(cmd));
            }

            if (embed.FieldsEmpty)
            {
                embed.WithDescription($"Na metodu **{prefix.PreventMassTags()}** nemáš žádná potřebná oprávnění.");
            }
            else
            {
                if (!string.IsNullOrEmpty(remarks))
                    embed.WithDescription(remarks);
            }

            return embed;
        }

        private EmbedFieldBuilder CreateCommandDetail(CommandInfo command)
        {
            var builder = new StringBuilder();

            if (command.Parameters.Count > 0)
            {
                builder
                    .Append("Parametry: ")
                    .AppendLine(string.Join(", ", command.Parameters.Select(o => o.Name)));
            }

            var summary = ProcessSummary(command);
            if (!string.IsNullOrEmpty(summary))
                builder.AppendLine(summary);

            if (!string.IsNullOrEmpty(command.Remarks))
                builder.Append("Poznámka: ").AppendLine(command.Remarks.Replace("{prefix}", CommandPrefix));

            return new EmbedFieldBuilder()
                .WithName(FormatAliases(command.Aliases))
                .WithValue(builder.Length == 0 ? "Bez parametrů a popisu" : builder.ToString());
        }

        private void ProcessSearchError(SearchResult result, string command)
        {
            switch (result.Error)
            {
                case CommandError.UnknownCommand:
                    throw new NotFoundException($"Je mi to líto, ale příkaz `{command}` neznám.");
                case CommandError.UnmetPrecondition:
                    throw new UnauthorizedAccessException(result.ErrorReason.PreventMassTags());
            }
        }

        private string FormatAliases(IReadOnlyCollection<string> aliases)
        {
            var aliasFields = aliases.Select(o => o.Split(' '));
            var builder = new List<string>();

            foreach (var group in aliasFields.GroupBy(o => o[0]))
            {
                var commands = group.Where(o => o.Length >= 2).Select(o => o[1]);
                builder.Add($"{group.Key} {string.Join(", ", commands)}");
            }

            return string.Join("\n", builder);
        }
    }
}
