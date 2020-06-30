using Discord.WebSocket;
using Grillbot.Database.Entity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> SetSelfUnverify(SocketGuildUser user, SocketGuild guild, string time, string[] subjects)
        {
            const string message = "Self unverify";

            using var scope = Provider.CreateScope();
            using var checker = scope.ServiceProvider.GetService<TempUnverifyChecker>();
            checker.Validate(user, guild, true);

            var timeParser = scope.ServiceProvider.GetService<TempUnverifyTimeParser>();
            var unverifyTime = timeParser.Parse(time);
            var unverify = await RemoveAccessAsync(user, unverifyTime, message, user, guild, true, subjects);

            unverify.InitTimer(ReturnAccess);
            Data.Add(unverify);

            return FormatMessageToChannel(
                new List<SocketGuildUser> { user },
                new List<TempUnverifyItem>() { unverify },
                message
            );
        }
    }
}
