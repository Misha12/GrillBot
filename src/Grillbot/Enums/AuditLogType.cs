using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum AuditLogType
    {
        [Display(Name = "Příkaz")]
        Command
    }
}
