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
            var body = new Bitmap(250, 105);
            using var graphics = Graphics.FromImage(body);

            using var profilePic = await UserHelper.DownloadProfilePictureAsync(user, 64, true);
            graphics.DrawImage(profilePic, new Rectangle(new Point(20, 10), new Size(64, 64)));

            using var peepoImage = Img.FromFile(config.ImagePath);
            graphics.DrawImage(peepoImage, new Point(115, -5));

            return body;
        }
    }
}
