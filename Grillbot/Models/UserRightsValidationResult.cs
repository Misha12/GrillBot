namespace Grillbot.Models
{
    public enum UserRightsValidationResult
    {
        OK,
        NotInGuild,
        BannedCommand,
        InvalidRights,
        OnlyAdmins
    }
}
