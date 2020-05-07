using Discord.WebSocket;
using Grillbot.Database.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> SetSelfUnverify(SocketGuildUser user, SocketGuild guild, string time, string[] subjects)
        {
            const string message = "Self unverify";

            using var checker = Factories.GetChecker();
            var currentUnverified = GetCurrentUnverifiedUserIDs();

            checker.Validate(user, guild, true, subjects.ToList(), currentUnverified);

            var timeParser = Factories.GetTimeParser();
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
