using Grillbot.Services.Unverify.Models;

namespace Grillbot.Models
{
    public class UnverifyInfo
    {
        public long ID { get; set; }
        public UnverifyUserProfile Profile { get; set; }
    }
}
