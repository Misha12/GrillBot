using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("files")]
    [Summary("Práce se soubory v databázi.")]
    [ModuleID(nameof(FilesModule))]
    public class FilesModule : BotModuleBase
    {
        public FilesModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("upload")]
        [Summary("Nahrání souboru. Vyžaduje přílohu.")]
        public async Task UploadFileAsync()
        {
            if (Context.Message.Attachments.Count == 0)
            {
                await ReplyAsync("Nebyla vložena příloha.");
                return;
            }

            var service = GetService<IGrillBotRepository>();
            foreach (var attachment in Context.Message.Attachments)
            {
                var content = await attachment.DownloadFileAsync();
                var file = await service.Service.FilesRepository.GetFileAsync(attachment.Filename);

                if (file == null)
                {
                    file = new Database.Entity.File()
                    {
                        Filename = attachment.Filename,
                        Content = content
                    };

                    await service.Service.AddAsync(file);
                }
                else
                {
                    file.Content = content;
                }
            }

            await service.Service.CommitAsync();
            await ReplyAsync(Context.Message.Attachments.Count > 1 ? "Soubory nahrány" : "Soubor nahrán");
        }

        [Command("list")]
        [Summary("Seznam souborů.")]
        public async Task ListFilesAsync()
        {
            using var service = GetService<IGrillBotRepository>();
            var filenames = await service.Service.FilesRepository.GetFilenames().ToListAsync();

            if (filenames.Count == 0)
            {
                await ReplyAsync("Žádný soubor neexistuje.");
                return;
            }

            await ReplyChunkedAsync(filenames.Select(o => $"> {o}").SplitInParts(10));
        }

        [Command("get")]
        [Summary("Získání souboru.")]
        public async Task GetFileAsync([Remainder] string filename)
        {
            using var service = GetService<IGrillBotRepository>();
            var file = await service.Service.FilesRepository.GetFileAsync(filename);

            if (file == null)
            {
                await ReplyAsync("Takový soubor neexistuje.");
                return;
            }

            await ReplyFileAsync(file.Content, file.Filename);
        }

        [Command("remove")]
        [Summary("Smazání souboru")]
        public async Task RemoveFileAsync([Remainder] string filename)
        {
            using var service = GetService<IGrillBotRepository>();
            var file = await service.Service.FilesRepository.GetFileAsync(filename);

            if (file == null)
            {
                await ReplyAsync("Takový soubor neexistuje.");
                return;
            }

            service.Service.Remove(file);
            await service.Service.CommitAsync();

            await ReplyAsync("Soubor smazán.");
        }
    }
}
