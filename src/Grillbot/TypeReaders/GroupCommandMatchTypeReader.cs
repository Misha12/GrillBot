using Discord.Commands;
using Grillbot.Database;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class GroupCommandMatchTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            using var scope = services.CreateScope();

            if (int.TryParse(input, out int methodID))
            {
                var methodsConfig = await GetConfigAsync(context, methodID, scope.ServiceProvider);

                if (methodsConfig == null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Hledaná konfigurace nebyla nalezena.");

                return TypeReaderResult.FromSuccess(new GroupCommandMatch()
                {
                    Command = methodsConfig.Command,
                    Group = methodsConfig.Group,
                    MethodID = methodID
                });
            }

            if (!input.Contains("/"))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Neplatný formát skupina/název.");

            var groupAndCommand = input.Split('/');

            if (!CommandExists(scope.ServiceProvider, context, groupAndCommand[0], groupAndCommand[1]))
                return TypeReaderResult.FromError(CommandError.ParseFailed, $"Neznámý příkaz `{input}`");

            var config = await GetConfigAsync(context, groupAndCommand[0], groupAndCommand[1], scope.ServiceProvider);

            return TypeReaderResult.FromSuccess(new GroupCommandMatch()
            {
                Command = groupAndCommand[1],
                Group = groupAndCommand[0],
                MethodID = config?.ID
            });
        }

        private async Task<MethodsConfig> GetConfigAsync(ICommandContext context, int methodID, IServiceProvider scopedProvider)
        {
            var repository = scopedProvider.GetService<IGrillBotRepository>();

            return await repository.ConfigRepository.GetAllMethods(context.Guild.Id)
                .SingleOrDefaultAsync(o => o.ID == methodID);
        }

        private async Task<MethodsConfig> GetConfigAsync(ICommandContext context, string group, string command, IServiceProvider scopedProvider)
        {
            var repository = scopedProvider.GetService<IGrillBotRepository>();

            return await repository.ConfigRepository.FindConfigAsync(context.Guild.Id, group, command);
        }

        private bool CommandExists(IServiceProvider scopedProvider, ICommandContext context, string group, string command)
        {
            var commandService = scopedProvider.GetService<CommandService>();

            var searchResult = commandService.Search(context, $"{group} {command}".Trim());
            return searchResult.IsSuccess;
        }
    }
}
