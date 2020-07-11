using Discord.Commands;
using System.Threading.Tasks;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.MemeImages;
using Grillbot.Attributes;
using System.IO;
using System.Drawing.Imaging;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.Dynamic;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [ModuleID("MemeImageModule")]
    [Name("Nudes a další zajímavé fotky")]
    public class MemeImageModule : BotModuleBase
    {
        private MemeImagesService Service { get; }

        public MemeImageModule(MemeImagesService service, ConfigRepository config) : base(configRepository: config)
        {
            Service = service;
        }

        [Command("nudes")]
        public async Task SendNudeAsync()
        {
            await SendAsync("nudes").ConfigureAwait(false);
        }

        [Command("notnudes")]
        public async Task SendNotNudesAsync()
        {
            await SendAsync("notnudes").ConfigureAwait(false);
        }

        private async Task SendAsync(string category)
        {
            var file = Service.GetRandomFile(Context.Guild, category);

            if (file == null)
            {
                await ReplyAsync("Nemám žádný obrázek.");
                return;
            }

            await Context.Channel.SendFileAsync(file);
        }

        [Command("peepolove")]
        public async Task PeepoloveAsync(Discord.IUser forUser = null)
        {
            if (forUser == null)
                forUser = Context.User;

            var config = GetMethodConfig<PeepoloveConfig>(null, "peepolove");

            using var bitmap = await Service.CreatePeepoloveAsync(forUser, config);
            using var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            await Context.Channel.SendFileAsync(ms, "peepolove.png");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Service.Dispose();

            base.Dispose(disposing);
        }
    }
}
