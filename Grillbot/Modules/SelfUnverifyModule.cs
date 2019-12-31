using Discord.Commands;
using Grillbot.Services.Preconditions;
using Grillbot.Services.TempUnverify;
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
            await DoAsync(async () =>
            {
                var user = Context.Guild.GetUser(Context.User.Id);
                var message = await UnverifyService.SetSelfUnverify(user, Context.Guild, time).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
