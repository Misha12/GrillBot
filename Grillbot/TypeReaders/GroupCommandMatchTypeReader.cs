using Discord.Commands;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Repository;
using Grillbot.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class GroupCommandMatchTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (int.TryParse(input, out int methodID))
            {
                var methodsConfig = GetConfig(context, methodID, services);

                if (methodsConfig == null)
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Hledaná konfigurace nebyla nalezena."));

                return Task.FromResult(TypeReaderResult.FromSuccess(new GroupCommandMatch()
                {
                    Command = methodsConfig.Command,
                    Group = methodsConfig.Group,
                    MethodID = methodID
                }));
            }

            if (!input.Contains("/"))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Neplatný formát skupina/název."));

            var groupAndCommand = input.Split('/');

            if (!CommandExists(services, context, groupAndCommand[0], groupAndCommand[1]))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Neznámý příkaz `{input}`"));

            var config = GetConfig(context, groupAndCommand[0], groupAndCommand[1], services);

            return Task.FromResult(TypeReaderResult.FromSuccess(new GroupCommandMatch()
            {
                Command = groupAndCommand[1],
                Group = groupAndCommand[0],
                MethodID = config?.ID
            }));
        }

        private MethodsConfig GetConfig(ICommandContext context, int methodID, IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<ConfigRepository>();

            return repository.GetMethod(context.Guild, methodID);
        }

        private MethodsConfig GetConfig(ICommandContext context, string group, string command, IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<ConfigRepository>();

            return repository.FindConfig(context.Guild.Id, group, command);
        }

        private bool CommandExists(IServiceProvider provider, ICommandContext context, string group, string command)
        {
            using var scope = provider.CreateScope();
            var commandService = scope.ServiceProvider.GetService<CommandService>();

            var searchResult = commandService.Search(context, $"{group} {command}");
            return searchResult.IsSuccess;
        }
    }
}
