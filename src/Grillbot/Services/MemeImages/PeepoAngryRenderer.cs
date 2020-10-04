using Discord;
using Grillbot.Helpers;
using Grillbot.Models.Config.Dynamic;
using System.Drawing;
using System.Threading.Tasks;
using Img = System.Drawing.Image;

namespace Grillbot.Services.MemeImages
{
    public class PeepoAngryRenderer
    {
        public async Task<Bitmap> RenderAsync(IUser user, PeepoAngryConfig config)
        {
            using var profilePic = await UserHelper.DownloadProfilePictureAsync(user, 128, true);

            var body = new Bitmap(300, 150);
            using var graphics = Graphics.FromImage(body);

            graphics.DrawImage(profilePic, new Point(10, 10));

            using var peepoImage = Img.FromFile(config.ImagePath);
            graphics.DrawImage(peepoImage, new Point(170, 40));

            return body;
        }
    }
}
