using Discord;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Microsoft.Extensions.Options;
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
        private Configuration AppConfig { get; }

        public MemeImagesService(ConfigRepository repository, IOptions<Configuration> options)
        {
            ConfigRepository = repository;
            Random = new Random();
            AppConfig = options.Value;
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

        public async Task<Img> CreatePeepoloveAsync(IUser forUser)
        {
            using var profileImage = await RenderProfileImageAsync(forUser);

            // Drawing canvas
            using var body = new Bitmap(Path.Combine(AppConfig.PeepoloveDir, "peepoBody.png"));
            using var graphics = Graphics.FromImage(body);

            graphics.RotateTransform(-0.4f);
            graphics.DrawImage(profileImage, 5, 512 - 200, 180, 180);
            graphics.RotateTransform(0.4f);
            graphics.DrawImage(Img.FromFile(Path.Combine(AppConfig.PeepoloveDir, "peepoHands.png")), 0, 0, 512, 512);

            graphics.DrawImage(body, new Point(0, 0));

            const int startY = 115;
            return (body as Img).CropImage(new Rectangle(0, startY, 512, 512 - startY));
        }

        private async Task<Img> RenderProfileImageAsync(IUser user)
        {
            var profileImageData = await user.DownloadAvatarAsync(256);
            using var profileImageStream = new MemoryStream(profileImageData);
            using var profileImage = Img.FromStream(profileImageStream);

            return profileImage.RoundCorners();
        }

        public void Dispose()
        {
            ConfigRepository.Dispose();
        }
    }
}
