using System.ComponentModel.DataAnnotations;

namespace Grillbot.Enums
{
    public enum WebAdminUserOrder
    {
        [Display(Name = "Body")]
        Points,

        Server,

        [Display(Name = "Udělené reakce")]
        GivenReactions,

        [Display(Name = "Získané reakce")]
        ObtainedReactions
    }
}
