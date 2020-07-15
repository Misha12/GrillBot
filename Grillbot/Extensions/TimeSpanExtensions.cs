using Discord;
using System;

namespace Grillbot.Extensions
{
#pragma warning disable S3358 // Ternary operators should not be nested
    public static class TimeSpanExtensions
    {
        public static string ToCzechLongTimeString(this TimeSpan ts)
        {
            var hoursCzMod = ts.Hours == 1 ? "a" : (ts.Hours > 1 && ts.Hours < 5) ? "y" : "";
            var minCzMod = ts.Minutes == 1 ? "a" : (ts.Minutes > 1 && ts.Minutes < 5) ? "y" : "";
            return $"{ts.Hours} hodin{hoursCzMod} a {ts.Minutes} minut{minCzMod}";
        }

        public static string ToFullCzechTimeString(this TimeSpan timeSpan)
        {
            if (timeSpan.Days == 0)
                return timeSpan.ToCzechLongTimeString();

            string daysCz = timeSpan.Days == 1 ? "den" : (timeSpan.Days > 1 && timeSpan.Days < 5 ? "dny" : "dní");
            return $"{timeSpan.Days} {daysCz} {timeSpan.ToCzechLongTimeString()}";
        }
    }
}