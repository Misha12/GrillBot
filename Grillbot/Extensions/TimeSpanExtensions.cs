using System;

namespace Grillbot.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToCzechLongTimeString(this TimeSpan ts)
        {
            var hoursCzMod = ts.Hours == 1 ? "a" : (ts.Hours > 1 && ts.Hours < 5) ? "y" : "";
            var minCzMod = ts.Minutes == 1 ? "a" : (ts.Minutes > 1 && ts.Minutes < 5) ? "y" : "";
            return $"{ts.Hours} hodin{hoursCzMod} a {ts.Minutes} minut{minCzMod}";
        }
    }
}