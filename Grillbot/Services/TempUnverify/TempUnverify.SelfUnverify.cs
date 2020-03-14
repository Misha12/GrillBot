using Discord.WebSocket;
using Grillbot.Database.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> SetSelfUnverify(SocketGuildUser user, SocketGuild guild, string time)
        {
            const string message = "Self unverify";

            CheckIfCanStartUnverify(new List<SocketGuildUser>() { user }, guild, true);

            var unverifyTime = ParseUnverifyTime(time);
            TempUnverifyItem unverify = await RemoveAccessAsync(user, unverifyTime, message, user, guild, true)
                .ConfigureAwait(false);

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
