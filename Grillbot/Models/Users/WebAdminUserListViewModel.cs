using System.Collections.Generic;

namespace Grillbot.Models.Users
{
    public class WebAdminUserListViewModel
    {
        public List<DiscordUser> Users { get; set; }
        public WebAdminUserOrder Order { get; set; }
        public bool SortDesc { get; set; }

        public WebAdminUserListViewModel(List<DiscordUser> users, WebAdminUserOrder order, bool sortDesc)
        {
            Users = users;
            Order = order;
            SortDesc = sortDesc;
        }
    }
}
