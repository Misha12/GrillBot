using Grillbot.Database.Entity.Users;
using System;

namespace Grillbot.Models.Users
{
    public class UserBirthday
    {
        public DateTime DateTime { get; set; }
        public bool AcceptAge { get; set; }

        public static bool HaveTodayBirthday(DateTime dateTime)
        {
            var today = DateTime.Today;
            return dateTime.Date.Day == today.Day && dateTime.Date.Month == today.Month;
        }

        public UserBirthday(BirthdayDate birthday)
        {
            DateTime = birthday.Date;
            AcceptAge = birthday.AcceptAge;
        }
    }
}
