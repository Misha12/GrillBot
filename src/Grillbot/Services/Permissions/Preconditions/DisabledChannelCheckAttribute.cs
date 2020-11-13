using Discord.Commands;
using Grillbot.Enums;
using Grillbot.Services.Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.Permissions.Preconditions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisabledChannelCheckAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var disabledChannels = GetDisabledChannels(services);

            if (disabledChannels.Contains(context.Channel.Id))
                return Task.FromResult(PreconditionResult.FromError("V tomto kanálu není provádění příkazů povoleno."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private List<ulong> GetDisabledChannels(IServiceProvider provider)
        {
            var disabledChannelsList = new List<ulong>();

            using var scope = provider.CreateScope();
            using var service = scope.ServiceProvider.GetService<ConfigurationService>();

            var disabledChannels = service.GetValue(GlobalConfigItems.DisabledChannels);
            if (!string.IsNullOrEmpty(disabledChannels))
                disabledChannelsList.AddRange(JsonConvert.DeserializeObject<List<ulong>>(disabledChannels));

            return disabledChannelsList;
        }
    }
}
