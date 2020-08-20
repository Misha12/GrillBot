using System;
using System.Drawing;

namespace Grillbot.Helpers
{
    public static class ColorHelper
    {
        public static bool IsDark(Color color)
        {
            var hsp = Math.Sqrt((0.299 * Math.Pow(color.R, 2)) + (0.587 * Math.Pow(color.G, 2)) + (0.114 * Math.Pow(color.B, 2)));
            return hsp <= 127.5D;
        }
    }
}
