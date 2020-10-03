namespace Grillbot.Models.Users
{
    public class WebAdminUserInfoViewModel
    {
        public DiscordUser User { get; set; }

        public WebAdminUserInfoViewModel(DiscordUser user)
        {
            User = user;
        }
    }
}
