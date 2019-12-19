using Discord;
using Grillbot.Extensions.Discord;

namespace Grillbot.Repository.Entity.UnverifyLog
{
    public class UnverifyLogDataBase
    {
        public string DestinationUser { get; set; }

        public void SetUser(IUser user)
        {
            DestinationUser = $"{user.GetShortName()} {user.Id}";
        }
    }
}
