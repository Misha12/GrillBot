using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Models.Config.Dynamic;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.MemeImages
{
    public class MemeImagesService
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private Random Random { get; }

        public MemeImagesService(IGrillBotRepository grillbotRepository)
        {
            GrillBotRepository = grillbotRepository;
            Random = new Random();
        }

        public async Task<byte[]> GetRandomFileAsync(SocketGuild guild, string category)
        {
            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "", category, false);
            var configData = config.GetData<MemeImagesConfig>();

            var filenamesQuery = GrillBotRepository.FilesRepository.GetFilenames().Where(o => o.StartsWith($"{category}_"));

            var filenames = await filenamesQuery
                .AsAsyncEnumerable()
                .Where(_ => configData.AllowedImageTypes.Any(type => type == Path.GetExtension(type)))
                .ToListAsync();

            if (filenames.Count == 0)
                return null;

            var filename = filenames[Random.Next(filenames.Count)];
            var file = await GrillBotRepository.FilesRepository.GetFileAsync(filename);

            return file?.Content;
        }
    }
}
