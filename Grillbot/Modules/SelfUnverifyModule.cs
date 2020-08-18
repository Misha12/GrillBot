using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Models.Embed;
using Grillbot.Services.TempUnverify;
using Grillbot.Services.Unverify;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("selfunverify")]
    [ModuleID("SelfUnverifyModule")]
    [Name("Odebrání přístupu")]
    public class SelfUnverifyModule : BotModuleBase
    {
        private TempUnverifyRoleManager RoleManager { get; }
        private UnverifyService Service { get; }

        public SelfUnverifyModule(TempUnverifyRoleManager roleManager, UnverifyService service)
        {
            RoleManager = roleManager;
            Service = service;
        }

        [Command("")]
        [Summary("Odebrání práv sám sobě.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d/M/y}, případně v ISO 8601. Např.: 30m, nebo `2020-08-17T23:59:59`.\nPopis: **m**: minuty, **h**: hodiny, " +
            "**d**: dny, **M**: měsíce, **y**: roky.\n\nJe možné si ponechat určité množství přístupů. Možnosti jsou k dispozici pomocí `{prefix}selfunverify defs`" +
            ", které bude možné si během doby odebraného přístupu ponechat.\nMinimální doba pro selfunverify je půl hodiny.\n\nCelý příkaz je pak vypadá např.:\n`{prefix}selfunverify 30m`, nebo " +
            "`{prefix}selfunverify 30m IPT ...`")]
        public async Task SetSelfUnverify(string time, params string[] subjects)
        {
            if (await SelfUnverifyRoutingAsync(time))
                return;

            try
            {
                if (!(Context.User is SocketGuildUser user))
                    return;

                var message = await Service.SetUnverifyAsync(user, time, "Self unverify", Context.Guild, user, true, subjects.ToList());
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
                case "defs":
                    await GetSubjectListAsync();
                    return true;
            }

            return false;
        }

        [Command("defs")]
        [Summary("Definice přístupů, co si může osoba ponechat.")]
        public async Task GetSubjectListAsync()
        {
            var config = RoleManager.GetSelfUnverifyConfig(Context.Guild);

            var embed = new BotEmbed(Context.User, title: "Ponechatelné role a kanály")
                .AddField("Max. počet ponechatelných", config.MaxRolesToKeep.FormatWithSpaces(), false);

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

        protected override void AfterExecute(CommandInfo command)
        {
            RoleManager.Dispose();
            Service.Dispose();

            base.AfterExecute(command);
        }
    }
}
