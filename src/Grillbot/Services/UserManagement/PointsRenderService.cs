using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class PointsRenderService
    {
        private Font PositionFont { get; } = new Font("Segoe UI", 45F);
        private Font NicknameFont { get; } = new Font("Segoe UI", 40F);
        private Font DiscriminatorFont { get; } = new Font("Segoe UI", 30F);
        private Font TitleTextFont { get; } = new Font("Segoe UI", 15F);

        private SolidBrush WhiteBrush { get; } = new SolidBrush(Color.White);

        public async Task<Bitmap> RenderAsync(Discord.IUser user, int position, long points)
        {
            var bitmap = new Bitmap(1000, 300);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            FillRectangle(graphics, new Rectangle(0, 0, bitmap.Width, bitmap.Height), Color.FromArgb(35, 39, 42), 15);
            FillRectangle(graphics, new Rectangle(50, 50, 900, 200), Color.FromArgb(100, 0, 0, 0), 15);

            var profileImage = await GetProfileImageAsync(user);
            graphics.DrawImage(profileImage, 70, 70, 160, 160);

            var positionTextSize = graphics.MeasureString($"#{position}", PositionFont);
            graphics.DrawString("BODY", TitleTextFont, WhiteBrush, new PointF(250, 192));
            graphics.DrawString(points.FormatWithSpaces(), PositionFont, new SolidBrush(Color.LightGray), new PointF(300, 150));
            var positionTitleTextSize = graphics.MeasureString("POZICE", TitleTextFont);
            graphics.DrawString("POZICE", TitleTextFont, WhiteBrush, new PointF(900 - positionTextSize.Width - positionTitleTextSize.Width, 192));
            graphics.DrawString($"#{position}", PositionFont, WhiteBrush, new PointF(900 - positionTextSize.Width, 150));

            var nickname = user.GetDisplayName(true).Cut(20);
            graphics.DrawString(nickname, NicknameFont, WhiteBrush, new PointF(250, 60));
            var nicknameSize = graphics.MeasureString(nickname, NicknameFont);
            graphics.DrawString($"#{user.Discriminator}", DiscriminatorFont, new SolidBrush(Color.DimGray), new PointF(250 + nicknameSize.Width, 76));

            return bitmap;
        }

        private void FillRectangle(Graphics graphics, Rectangle rect, Color color, int radius = 1)
        {
            var path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddLine(rect.X + radius, rect.Y, rect.X + rect.Width - radius, rect.Y);
            path.AddArc(rect.X + rect.Width - 2 * radius, rect.Y, 2 * radius, 2 * radius, 270, 90);
            path.AddLine(rect.X + rect.Width, rect.Y + radius, rect.X + rect.Width, rect.Y + rect.Height - radius);
            path.AddArc(rect.X + rect.Width - 2 * radius, rect.Y + rect.Height - 2 * radius, radius + radius, radius + radius, 0, 91);
            path.AddLine(rect.X + radius, rect.Y + rect.Height, rect.X + rect.Width - radius, rect.Y + rect.Height);
            path.AddArc(rect.X, rect.Y + rect.Height - 2 * radius, 2 * radius, 2 * radius, 90, 91);

            path.CloseFigure();
            graphics.FillPath(new SolidBrush(color), path);
        }

        private async Task<Image> GetProfileImageAsync(Discord.IUser user)
        {
            var profileImageData = await user.DownloadAvatarAsync(128);
            using var profileImageStream = new MemoryStream(profileImageData);
            using var profileImage = Image.FromStream(profileImageStream);

            return profileImage.RoundCorners();
        }
    }
}
