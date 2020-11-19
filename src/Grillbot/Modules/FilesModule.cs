using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("files")]
    [Summary("Práce se soubory v databázi.")]
    [ModuleID(nameof(FilesModule))]
    public class FilesModule : BotModuleBase
    {
        private FilesRepository FilesRepository { get; }

        public FilesModule(FilesRepository repository)
        {
            FilesRepository = repository;
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

            foreach (var attachment in Context.Message.Attachments)
            {
                var content = await attachment.DownloadFileAsync();
                await FilesRepository.UploadFileAsync(attachment.Filename, content);
            }

            await ReplyAsync(Context.Message.Attachments.Count > 1 ? "Soubory nahrány" : "Soubor nahrán");
        }

        [Command("list")]
        [Summary("Seznam souborů.")]
        public async Task ListFilesAsync()
        {
            var filenames = await FilesRepository.GetFilenames().ToListAsync();

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
            var file = await FilesRepository.GetFileAsync(filename);

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
            var file = await FilesRepository.GetFileAsync(filename);

            if (file == null)
            {
                await ReplyAsync("Takový soubor neexistuje.");
                return;
            }

            FilesRepository.Remove(file);
            await FilesRepository.SaveChangesAsync();

            await ReplyAsync("Soubor smazán.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                FilesRepository.Dispose();

            base.Dispose(disposing);
        }
    }
}
