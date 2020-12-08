using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum AuditLogType
    {
        [Display(Name = "Příkaz")]
        Command,

        [Display(Name = "Uživatel opustil server")]
        UserLeft,

        [Display(Name = "Uživatel se připojil na server")]
        UserJoined,

        [Display(Name = "Zpráva editována")]
        MessageEdited,

        [Display(Name = "Zpráva smazána")]
        MessageDeleted
    }
}
