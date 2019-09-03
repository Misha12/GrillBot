using System;

namespace Grillbot.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToLocaleDatetime(this DateTime dateTime) => dateTime.ToString("dd. MM. yyyy HH:mm:ss");
    }
}
