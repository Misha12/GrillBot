using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Grillbot.Extensions
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Create image with rounded corners
        /// </summary>
        /// <see cref="https://stackoverflow.com/a/1759073"/>
        /// <see cref="https://stackoverflow.com/a/19053520"/>
        public static Image RoundImage(this Image original)
        {
            var roundedImage = new Bitmap(original.Width, original.Height, original.PixelFormat);
            roundedImage.MakeTransparent();

            using var g = Graphics.FromImage(roundedImage);
            using var gp = new GraphicsPath();

            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var brush = new TextureBrush(original);
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

        /// <summary>
        /// Resizes image
        /// </summary>
        /// <remarks>https://stackoverflow.com/a/24199315</remarks>
        static public Image ResizeImage(this Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        static public List<Image> SplitGifIntoFrames(this Image image)
        {
            var frames = new List<Image>();

            for (int i = 0; i < image.GetFrameCount(FrameDimension.Time); i++)
            {
                image.SelectActiveFrame(FrameDimension.Time, i);
                frames.Add(new Bitmap(image));
            }

            return frames;
        }

        static public int CalculateGifDelay(this Image image)
        {
            var item = image.GetPropertyItem(0x5100); // FrameDelay in libgdi+.
            return item.Value[0] + (item.Value[1] * 256);
        }
    }
}
