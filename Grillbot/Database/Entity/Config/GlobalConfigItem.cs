using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Config
{
    [Table("GlobalConfig")]
    public class GlobalConfigItem
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
