using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Exceptions;
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
        public SelfUnverifyModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("")]
        [Summary("Odebrání práv sám sobě.")]
        [Remarks("Parametr time je ve formátu {cas}{m/h/d/M/y}, případně v ISO 8601. Např.: 30m, nebo `2020-08-17T23:59:59`.\nPopis: **m**: minuty, **h**: hodiny, " +
            "**d**: dny, **M**: měsíce, **y**: roky.\n\nJe možné si ponechat určité množství přístupů. Možnosti jsou k dispozici pomocí `{prefix}selfunverify defs`" +
            ", které bude možné si během doby odebraného přístupu ponechat.\nMinimální doba pro selfunverify je půl hodiny.\nNa self unverify se nevztahuje imunita." +
            "\n\nCelý příkaz je pak vypadá např.:\n`{prefix}selfunverify 30m`, nebo `{prefix}selfunverify 30m IPT ...`")]
        public async Task SetSelfUnverify(string time, params string[] subjects)
        {
            if (await SelfUnverifyRoutingAsync(time, subjects))
                return;

            try
            {
                if (Context.User is not SocketGuildUser user)
                    return;

                using var service = GetService<UnverifyService>();

                var message = await service.Service.SetUnverifyAsync(user, time, "Self unverify", Context.Guild, user, true, subjects.ToList());
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

        private async Task<bool> SelfUnverifyRoutingAsync(string route, string[] data)
        {
            switch (route)
            {
                case "defs":
                    await GetDefsListAsync();
                    return true;
                case "addDefs":
                    if (data.Length < 2)
                        throw new ThrowHelpException();
                    await AddDefinitionsAsync(data[0], data.Skip(1).ToArray());
                    return true;
                case "removeDefs":
                    if (data.Length < 2)
                        throw new ThrowHelpException();
                    await RemoveDefinitionsAsync(data[0], data.Skip(1).ToArray());
                    return true;
            }

            return false;
        }

        [Command("defs")]
        [Summary("Definice přístupů, co si může uživatel ponechat.")]
        public async Task GetDefsListAsync()
        {
            var config = await GetMethodConfigAsync<SelfUnverifyConfig>("selfunverify", null);

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

        [Command("addDefs")]
        [Summary("Přidání definic přístupu, které si lze ponechat.")]
        public async Task AddDefinitionsAsync(string group, params string[] values)
        {
            using var service = GetService<UnverifyService>();

            await service.Service.AddSelfunverifyDefinitionsAsync(Context.Guild, group, values);
            await ReplyAsync($"Definice přidán{(values.Length > 1 ? "y" : "a")}");
        }

        [Command("removeDefs")]
        [Summary("Odebrání definic přístupu, které si lze ponechat.")]
        public async Task RemoveDefinitionsAsync(string group, params string[] values)
        {
            using var service = GetService<UnverifyService>();

            var removedDefs = await service.Service.RemoveSelfunverifyDefinitions(Context.Guild, group, values);

            var message = string.Join(Environment.NewLine, new[]
            {
                $"Skupina: `{group}`",
                removedDefs.Item1.Count == 0 ? null : $"Smazané: {string.Join(", ", removedDefs.Item1.Select(o => $"`{o}`"))}",
                removedDefs.Item2.Count == 0 ? null : $"Již neexistovaly: {string.Join(", ", removedDefs.Item2.Select(o => $"`{o}`"))}"
            }.Where(o => o != null));

            await ReplyAsync(message);
        }
    }
}
