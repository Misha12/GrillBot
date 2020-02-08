using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> SetSelfUnverify(SocketGuildUser user, SocketGuild guild, string time)
        {
            CheckIfCanStartUnverify(new List<SocketGuildUser>() { user }, guild, true);

            var unverifyTime = ParseUnverifyTime(time);
            TempUnverifyItem unverify;
            using (var repository = new GrillBotRepository(Config))
            {
                unverify = await RemoveAccessAsync(repository, user, unverifyTime, "Self unverify", user, guild, true)
                    .ConfigureAwait(false);
            }

            unverify.InitTimer(ReturnAccess);
            Data.Add(unverify);

            return FormatMessageToChannel(
                new List<SocketGuildUser> { user },
                new List<TempUnverifyItem>() { unverify },
                "Self unverify"
            );
        }
    }
}
