using System;

namespace Grillbot.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToLocaleDatetime(this DateTime dateTime) => dateTime.ToString("dd. MM. yyyy HH:mm:ss");

        public static int ComputeDateAge(this DateTime dateTime)
        {
            var today = DateTime.Today;
            var age = today.Year - dateTime.Year;
            if (dateTime.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
