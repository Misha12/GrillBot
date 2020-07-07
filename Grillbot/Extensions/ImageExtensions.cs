using System.Drawing;
using System.Drawing.Drawing2D;

namespace Grillbot.Extensions
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Create image with rounded corners
        /// </summary>
        /// <see cref="https://stackoverflow.com/a/1759073"/>
        /// <see cref="https://stackoverflow.com/a/19053520"/>
        public static Image RoundCorners(this Image original)
        {
            var roundedImage = new Bitmap(original.Width, original.Height, original.PixelFormat);
            roundedImage.MakeTransparent();

            using var g = Graphics.FromImage(roundedImage);
            using var gp = new GraphicsPath();

            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Brush brush = new TextureBrush(original);
            gp.AddEllipse(0, 0, original.Width, original.Height);
            g.FillPath(brush, gp);

            return roundedImage;
        }

        public static Image CropImage(this Image original, Rectangle newScreen)
        {
            var result = new Bitmap(newScreen.Width, newScreen.Height);
            using var graphics = Graphics.FromImage(result);

            graphics.DrawImage(original, 0, 0, newScreen, GraphicsUnit.Pixel);
            return result;
        }
    }
}
