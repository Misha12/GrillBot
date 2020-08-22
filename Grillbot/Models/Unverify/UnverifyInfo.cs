using Grillbot.Services.Unverify.Models;

namespace Grillbot.Models.Unverify
{
    public class UnverifyInfo
    {
        public long ID { get; set; }
        public UnverifyUserProfile Profile { get; set; }
    }
}
