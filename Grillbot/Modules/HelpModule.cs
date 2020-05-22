using Discord.Commands;
using System;
using System.Threading.Tasks;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Options;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services;
using Grillbot.Exceptions;

namespace Grillbot.Modules
{
    [Name("Nápověda")]
    [Group("grillhelp")]
    [Alias("help")]
    [RequirePermissions]
    public class HelpModule : BotModuleBase
    {
        private HelpEmbedRenderer Renderer { get; }

        public HelpModule(IOptions<Configuration> config, PaginationService paginationService,
            HelpEmbedRenderer renderer) : base(config, paginationService: paginationService)
        {
            Renderer = renderer;
        }

        [Command("")]
        [Summary("Globální nápověda")]
        public async Task HelpAsync()
        {
            var embed = await Renderer.RenderSummaryHelpAsync(Context);
            await SendPaginatedEmbedAsync(embed);
        }

        [Command("")]
        [Summary("Nápověda k jednomu příkazu.")]
        public async Task HelpAsync([Remainder] string command)
        {
            try
            {
                var result = await Renderer.RenderCommandHelpAsync(command, Context);
                await ReplyAsync(embed: result.Build());
            }
            catch (Exception ex)
            {
                if (ex is NotFoundException || ex is UnauthorizedAccessException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }
    }
}
