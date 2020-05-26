using Grillbot.Database.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.Users
{
    public class UserBirthday
    {
        public DateTime DateTime { get; set; }
        public bool AcceptAge { get; set; }

        public bool HaveTodayBirthday()
        {
            var today = DateTime.Today;
            return DateTime.Date.Day == today.Day && DateTime.Date.Month == today.Month;
        }

        public UserBirthday(BirthdayDate birthday)
        {
            DateTime = birthday.Date;
            AcceptAge = birthday.AcceptAge;
        }
    }
}
