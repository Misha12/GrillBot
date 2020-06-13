using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("Statistics")]
    public class StatisticItem
    {
        [Key]
        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        public int WebAdminLoginCount { get; set; }
        public int ApiCallCount { get; set; }
    }
}
