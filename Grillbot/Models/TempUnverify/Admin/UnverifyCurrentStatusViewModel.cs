using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyCurrentStatusViewModel
    {
        public List<CurrentUnverifiedUser> CurrentUnverifiedUsers { get; }

        public UnverifyCurrentStatusViewModel(List<CurrentUnverifiedUser> currentUnverifiedUsers)
        {
            CurrentUnverifiedUsers = currentUnverifiedUsers;
        }
    }
}
