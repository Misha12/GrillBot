using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.Dynamic;
using System;
using System.IO;
using System.Linq;

namespace Grillbot.Services.MemeImages
{
    public class MemeImagesService : IDisposable
    {
        private ConfigRepository ConfigRepository { get; }
        private Random Random { get; }

        public MemeImagesService(ConfigRepository repository)
        {
            ConfigRepository = repository;
            Random = new Random();
        }

        public string GetRandomFile(SocketGuild guild, string category)
        {
            var config = ConfigRepository.FindConfig(guild.Id, "", category);
            var configData = config.GetData<MemeImagesConfig>();

            var files = Directory.GetFiles(configData.Path)
                .Where(file => configData.AllowedImageTypes.Any(type => type == Path.GetExtension(file)))
                .ToList();

            if (files.Count == 0)
                return null;

            var randomValue = Random.Next(files.Count);
            return files[randomValue];
        }

        public void Dispose()
        {
            ConfigRepository.Dispose();
        }
    }
}
