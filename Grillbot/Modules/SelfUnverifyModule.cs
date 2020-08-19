using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Embed;
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
        private UnverifyService Service { get; }

        public SelfUnverifyModule(UnverifyService service, ConfigRepository configRepository) : base(configRepository: configRepository)
        {
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
            var config = GetMethodConfig<SelfUnverifyConfig>("selfunverify", null);

            var embed = new BotEmbed(Context.User, title: "Ponechatelné role a kanály")
                .AddField("Max. počet ponechatelných", config.MaxRolesToKeep.FormatWithSpaces(), false);

            foreach (var group in config.RolesToKeep.GroupBy(o => string.Join("|", o.Value)))
            {
                var keys = string.Join(", ", group.AsEnumerable().Select(o => o.Key == "_" ? "Ostatní" : o.Key));
                var parts = group.First().Value.SplitInParts(50);

                foreach (var part in parts)
                {
                    embed.AddField(keys, string.Join(", ", part.Select(o => o.ToUpper())), false);
                }
            }

            await ReplyAsync(embed: embed.Build());
        }

        protected override void AfterExecute(CommandInfo command)
        {
            Service.Dispose();

            base.AfterExecute(command);
        }
    }
}
