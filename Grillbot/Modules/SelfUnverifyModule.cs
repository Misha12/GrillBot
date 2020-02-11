using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Preconditions;
using Grillbot.Services.TempUnverify;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("selfunverify")]
    public class SelfUnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }

        public SelfUnverifyModule(TempUnverifyService unverifyService)
        {
            UnverifyService = unverifyService;
        }

        [Command("")]
        [Summary("Odebrání práv sám sobě.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d}. Např.: 30m.\nPopis: m: minuty, h: hodiny, d: dny.")]
        public async Task SetSelfUnverify(string time)
        {
            await DoAsync(async () =>
            {
                var user = await Context.Guild.GetUserFromGuildAsync(Context.User.Id.ToString());
                var message = await UnverifyService.SetSelfUnverify(user, Context.Guild, time).ConfigureAwait(false);
                await ReplyAsync(message).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
