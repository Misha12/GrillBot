using Discord.Commands;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("selfunverify")]
    [RequirePermissions("SelfUnverify", DisabledForPM = true, BoosterAllowed = true)]
    public class SelfUnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }

        public SelfUnverifyModule(TempUnverifyService unverifyService)
        {
            UnverifyService = unverifyService;
        }

        [Command("")]
        [Summary("Odebrání práv sám sobě.")]
        [Remarks("Parametr time je ve formátu {cas}{s/m/h/d}. Např.: 30s.\nPopis: s: sekundy, m: minuty, h: hodiny, d: dny.")]
        public async Task SetSelfUnverify(string time)
        {
            switch(time)
            {
                case "status":
                    await StatusSelfUnverify().ConfigureAwait(false);
                    return;
                case "remove":
                    await RemoveSelfUnverify().ConfigureAwait(false);
                    return;
            }

            await DoAsync(async () =>
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                var message = await UnverifyService.SetSelfUnverify(user, Context.Guild, time).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            });
        }

        [Command("update")]
        [Summary("Aktualizace času pro self unverify.")]
        [Remarks("Parametr time je ve formátu {cas}{s/m/h/d}. Např.: 30s.\nPopis: s: sekundy, m: minuty, h: hodiny, d: dny.")]
        public async Task UpdateSelfUnverify(string time)
        {
            await DoAsync(async () =>
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                var message = await UnverifyService.UpdateSelfUnverify(user, time);
                await ReplyAsync(message).ConfigureAwait(false);
            });
        }

        [Command("remove")]
        [Summary("Odebrání self unverify.")]
        public async Task RemoveSelfUnverify()
        {
            await DoAsync(async () =>
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                var message = await UnverifyService.ReturnSelfUnverifyAccess(user);
                await ReplyAsync(message).ConfigureAwait(false);
            });
        }

        [Command("status")]
        [Summary("Informace o zbýbajícím času.")]
        public async Task StatusSelfUnverify()
        {
            await DoAsync(async () =>
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                var status = UnverifyService.SelfUnverifyStatus(user);
                await ReplyAsync(status).ConfigureAwait(false);
            });
        }
    }
}
