namespace Grillbot.Services.Permissions
{
    public enum PermissionsResult
    {
        Success,
        MethodNotFound,
        PMNotAllowed,
        OnlyAdmins,
        UserIsBanned,
        MissingPermissions,
        NoPermissions
    }
}
