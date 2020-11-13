namespace Grillbot.Helpers
{
    public static class FormatHelper
    {
        public static string FormatUsersCountCz(long count)
        {
            if (count == 1)
                return "1 uživatel";

            return count > 1 && count < 5 ? $"{count} uživatelé" : $"{count} uživatelů";
        }
    }
}
