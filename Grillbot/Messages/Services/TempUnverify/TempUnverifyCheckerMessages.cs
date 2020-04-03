namespace Grillbot.Messages.Services.TempUnverify
{
    public class TempUnverifyCheckerMessages
    {
        public const string SubjectsOverMaximum = "Je možné si ponechat maximálně {0} rolí.";
        public const string InvalidSubjectRole = "`{0}` není předmětová role.";
        public const string ServerOwner = "Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.";
        public const string UserHaveUnverify = "Nelze provést odebrání přístupu, protože uživatel **{0}** již má odebraný přístup.";
        public const string UserHaveHigherRoles = "Nelze provést odebírání přístupu, protože uživatel **{0}** má vyšší role. **({1})**";
        public const string BotAdmin = "Nelze provést odebrání přístupu, protože uživatel **{0}** je administrátor bota.";
    }
}
