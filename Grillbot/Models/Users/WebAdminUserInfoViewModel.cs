namespace Grillbot.Models.Users
{
    public class WebAdminUserInfoViewModel
    {
        public WebAdminUser User { get; set; }

        public WebAdminUserInfoViewModel(WebAdminUser user)
        {
            User = user;
        }
    }
}
