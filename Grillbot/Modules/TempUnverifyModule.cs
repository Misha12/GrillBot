using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Group("unverify")]
    [Name("Odebrání přístupu.")]
    [RequirePermissions("TempUnverify")]
    public class TempUnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }

        public TempUnverifyModule(TempUnverifyService unverifyService)
        {
            UnverifyService = unverifyService;
        }

        [Command("")]
        [Summary("Dočasné odebrání rolí.")]
        [Remarks("Parmetr time je ve formátu {cas}{s/m/h/d}. Např.: 30s.\nPopis: s: sekundy, m: minuty, h: hodiny, d: dny.\n" +
            "Dále lze uvést důvod, proč daná osoba přišla o role. A nakonec seznam (mentions) uživatelů.\n" +
            "Celý příkaz je pak vypadá např.:\n{prefix}unverify 30s Přišel jsi o role @User1#1234 @User2#1354 ...\n" +
            "{prefix}unverify 30s @User1#1234 @User2#1354 ...")]
        public async Task SetUnverifyAsync(string time, [Remainder] string data = null)
        {
            // Simply hack, because command routing cannot distinguish between a parameter and a function.
            switch (time)
            {
                case "list":
                    await ListUnverifyAsync();
                    return;
                case "remove":
                    if (string.IsNullOrEmpty(data)) return;
                    await RemoveUnverifyAsync(Convert.ToInt32(data.Split(' ')[0]));
                    return;
                case "update":
                    if (string.IsNullOrEmpty(data)) return;
                    var fields = data.Split(' ');
                    if (fields.Length < 2)
                    {
                        await ReplyAsync("Chybí parametry.");
                        return;
                    }
                    await UpdateUnverifyAsync(Convert.ToInt32(fields[0]), fields[1]);
                    return;
            }

            await DoAsync(async () =>
            {
                var usersToUnverify = Context.Message.MentionedUsers.OfType<SocketGuildUser>().ToList();

                if(usersToUnverify.Count > 0)
                {
                    var message = await UnverifyService.RemoveAccessAsync(usersToUnverify, time, data, Context.Guild);
                    await ReplyAsync(message);
                }
            });
        }

        [Command("remove")]
        [Summary("Předčasné vrácení rolí.")]
        public async Task RemoveUnverifyAsync(int id)
        {
            await DoAsync(async () =>
            {
                var message = await UnverifyService.ReturnAccessAsync(id);
                await ReplyAsync(message);
            });
        }

        [Command("list")]
        [Summary("Seznam všech lidí, co má dočasně odebrané role.")]
        public async Task ListUnverifyAsync()
        {
            var callerUsername = GetUsersShortName(Context.Message.Author);
            var callerAvatar = GetUserAvatarUrl(Context.Message.Author);

            await DoAsync(async () =>
            {
                var embed = await UnverifyService.ListPersonsAsync(callerUsername, callerAvatar);
                await ReplyAsync(embed: embed.Build());
            });
        }

        [Command("update")]
        [Summary("Aktualizace času u záznamu o dočasném odebrání rolí.")]
        public async Task UpdateUnverifyAsync(int id, string time)
        {
            await DoAsync(async () =>
            {
                var message = await UnverifyService.UpdateUnverifyAsync(id, time);
                await ReplyAsync(message);
            });
        }
    }
}
