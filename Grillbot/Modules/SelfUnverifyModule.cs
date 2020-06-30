using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.TempUnverify;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("selfunverify")]
    [ModuleID("SelfUnverifyModule")]
    [Name("Odebrání přístupu")]
    public class SelfUnverifyModule : BotModuleBase
    {
        private TempUnverifyService UnverifyService { get; }
        private TempUnverifyRoleManager RoleManager { get; }

        public SelfUnverifyModule(TempUnverifyService unverifyService, TempUnverifyRoleManager roleManager)
        {
            UnverifyService = unverifyService;
            RoleManager = roleManager;
        }

        [Command("")]
        [Summary("Odebrání práv sám sobě.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d}. Např.: 30m.\nPopis: m: minuty, h: hodiny, d: dny.\nJe možné zadat maximálně 5 rolí " +
            ", které bude možné si během doby odebraného přístupu ponechat.\nMinimální doba pro selfunverify je půl hodiny.")]
        public async Task SetSelfUnverify(string time, params string[] subjects)
        {
            if (await SelfUnverifyRoutingAsync(time))
                return;

            try
            {
                var user = await Context.Guild.GetUserFromGuildAsync(Context.User.Id);
                var message = await UnverifyService.SetSelfUnverify(user, Context.Guild, time, subjects);
                await ReplyAsync(message);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is ValidationException || ex is FormatException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        private async Task<bool> SelfUnverifyRoutingAsync(string route)
        {
            switch (route)
            {
                case "roles":
                    await GetSubjectListAsync();
                    return true;
            }

            return false;
        }

        [Command("roles")]
        [Summary("Seznam rolí, co si může osoba ponechat.")]
        public async Task GetSubjectListAsync()
        {
            var config = RoleManager.GetSelfUnverifyConfig(Context.Guild);

            var embed = new BotEmbed(Context.User, title: "Ponechatelné role")
                .AddField("Počet ponechatelných", config.MaxRolesToKeep.FormatWithSpaces(), false);

            foreach (var group in config.RolesToKeep)
            {
                var parts = group.Value.SplitInParts(50);
                var key = group.Key == "_" ? "Ostatní" : group.Key;

                foreach (var part in parts)
                {
                    embed.AddField(key, string.Join(", ", part.Select(o => o.ToUpper())), false);
                }
            }

            await ReplyAsync(embed: embed.Build());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                RoleManager.Dispose();

            base.Dispose(disposing);
        }
    }
}
