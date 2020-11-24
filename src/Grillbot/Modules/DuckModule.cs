using System;
using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Services.Duck;

namespace Grillbot.Modules
{
    [Name("Stav Kachny")]
    [Group("kachna")]
    [ModuleID("DuckModule")]
    public class DuckModule : BotModuleBase
    {
        private DuckEmbedRenderer Renderer { get; }

        public DuckModule(DuckEmbedRenderer renderer, IServiceProvider provider) : base(provider: provider)
        {
            Renderer = renderer;
        }

        [Command("", true)]
        [Summary("Zjištění aktuálního stavu kachny")]
        public async Task GetDuckInfoAsync()
        {
            try
            {
                using var loader = GetService<DuckDataLoader>();

                var config = await GetMethodConfigAsync<DuckConfig>("kachna", "");
                var duckData = await loader.Service.GetDuckCurrentState(config);

                var embed = Renderer.RenderEmbed(duckData, Context.User, config);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch(WebException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}