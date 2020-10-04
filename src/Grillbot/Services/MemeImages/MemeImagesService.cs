using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Helpers;
using Grillbot.Models.Config.Dynamic;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Img = System.Drawing.Image;

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

        public async Task<Img> CreatePeepoloveAsync(IUser forUser, PeepoloveConfig config)
        {
            using var profileImage = await UserHelper.DownloadProfilePictureAsync(forUser, config.ProfilePicSize, true);

            // Drawing canvas
            using var body = new Bitmap(config.BodyPath);
            using var graphics = Graphics.FromImage(body);

            graphics.RotateTransform(-config.Rotate);
            graphics.DrawImage(profileImage, config.ProfilePicRect);
            graphics.RotateTransform(config.Rotate);
            graphics.DrawImage(Img.FromFile(config.HandsPath), config.Screen);

            graphics.DrawImage(body, new Point(0, 0));
            return (body as Img).CropImage(config.CropRect);
        }

        public Task<Bitmap> PeepoAngryAsync(IUser forUser, PeepoAngryConfig config)
        {
            var renderer = new PeepoAngryRenderer();
            return renderer.RenderAsync(forUser, config);
        }

        public void Dispose()
        {
            ConfigRepository.Dispose();
        }
    }
}
