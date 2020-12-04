using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum AuditLogOrder
    {
        [Display(Name = "Chronologicky")]
        DateTime,

        Server,

        [Display(Name = "Uživatel")]
        User
    }
}
