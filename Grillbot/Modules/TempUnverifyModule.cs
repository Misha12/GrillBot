using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("unverify")]
    [Name("Odebrání přístupu.")]
    [RequirePermissions("TempUnverify", DisabledForPM = true)]
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
            "Dále je důvod, proč daná osoba přišla o role. A nakonec seznam (mentions) uživatelů.\n" +
            "Celý příkaz je pak vypadá např.:\n{prefix}unverify 30s Přišel jsi o role @User1#1234 @User2#1354 ...")]
        public async Task SetUnverifyAsync(string time, [Remainder] string reasonAndUserMentions = null)
        {
            // Simply hack, because command routing cannot distinguish between a parameter and a function.
            switch (time)
            {
                case "list":
                    await ListUnverifyAsync().ConfigureAwait(false);
                    return;
                case "remove":
                    if (string.IsNullOrEmpty(reasonAndUserMentions)) return;
                    await RemoveUnverifyAsync(Convert.ToInt32(reasonAndUserMentions.Split(' ')[0])).ConfigureAwait(false);
                    return;
                case "update":
                    if (string.IsNullOrEmpty(reasonAndUserMentions)) return;
                    var fields = reasonAndUserMentions.Split(' ');
                    if (fields.Length < 2)
                    {
                        await ReplyAsync("Chybí parametry.").ConfigureAwait(false);
                        return;
                    }
                    await UpdateUnverifyAsync(Convert.ToInt32(fields[0]), fields[1]).ConfigureAwait(false);
                    return;
            }

            await DoAsync(async () =>
            {
                var usersToUnverify = Context.Message.MentionedUsers.OfType<SocketGuildUser>().ToList();

                if(usersToUnverify.Count > 0)
                {
                    var message = await UnverifyService.RemoveAccessAsync(usersToUnverify, time,
                        reasonAndUserMentions, Context.Guild, Context.User).ConfigureAwait(false);
                    await ReplyAsync(message).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        [Command("remove")]
        [Summary("Předčasné vrácení rolí.")]
        public async Task RemoveUnverifyAsync(int id)
        {
            await DoAsync(async () =>
            {
                var message = await UnverifyService.ReturnAccessAsync(id, Context.User).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("list")]
        [Summary("Seznam všech lidí, co má dočasně odebrané role.")]
        public async Task ListUnverifyAsync()
        {
            var callerUsername = Context.Message.Author.GetShortName();
            var callerAvatar = Context.Message.Author.GetUserAvatarUrl();

            await DoAsync(async () =>
            {
                var embed = await UnverifyService.ListPersonsAsync(callerUsername, callerAvatar).ConfigureAwait(false);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("UserStatus")]
        [Summary("Informace o odebrání rolí pro určitého uživatele.")]
        [Remarks("Zadává se ID uživatele.")]
        public async Task UserStatusAsync(ulong userId)
        {
            var callerUsername = Context.Message.Author.GetShortName();
            var callerAvatar = Context.Message.Author.GetUserAvatarUrl();

            await DoAsync(async () =>
            {
                var embed = await UnverifyService.GetPersonUnverifyStatus(callerUsername, callerAvatar, userId).ConfigureAwait(false);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("update")]
        [Summary("Aktualizace času u záznamu o dočasném odebrání rolí.")]
        public async Task UpdateUnverifyAsync(int id, string time)
        {
            await DoAsync(async () =>
            {
                var message = await UnverifyService.UpdateUnverifyAsync(id, time, Context.User).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
