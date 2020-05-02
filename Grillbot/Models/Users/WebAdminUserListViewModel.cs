using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListViewModel
    {
        public List<WebAdminUser> Users { get; set; }

        public WebAdminUserListViewModel(List<WebAdminUser> users)
        {
            Users = users;
        }
    }
}
