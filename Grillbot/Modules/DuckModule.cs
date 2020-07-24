using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Services.Duck;
using Microsoft.Extensions.Options;

namespace Grillbot.Modules
{
    [Name("Stav Kachny")]
    [Group("kachna")]
    [ModuleID("DuckModule")]
    public class DuckModule : BotModuleBase
    {
        private DuckDataLoader DuckDataLoader { get; }
        private DuckEmbedRenderer Renderer { get; }

        public DuckModule(IOptions<Configuration> config, ConfigRepository repository, DuckEmbedRenderer renderer,
            DuckDataLoader duckDataLoader) : base(config, repository)
        {
            DuckDataLoader = duckDataLoader;
            Renderer = renderer;
        }

        [Command("", true)]
        [Summary("Zjištění aktuálního stavu kachny")]
        public async Task GetDuckInfoAsync()
        {
            try
            {
                var config = GetMethodConfig<DuckConfig>("kachna", "");
                var duckData = await DuckDataLoader.GetDuckCurrentState(config);

                var embed = Renderer.RenderEmbed(duckData, Context.User, config);
                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch(WebException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DuckDataLoader.Dispose();

            base.Dispose(disposing);
        }
    }
}