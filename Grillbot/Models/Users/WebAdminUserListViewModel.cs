using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListViewModel
    {
        public List<WebAdminUser> Users { get; set; }
        public WebAdminUserOrder Order { get; set; }
        public bool SortDesc { get; set; }

        public WebAdminUserListViewModel(List<WebAdminUser> users, WebAdminUserOrder order, bool sortDesc)
        {
            Users = users;
            Order = order;
            SortDesc = sortDesc;
        }
    }
}
