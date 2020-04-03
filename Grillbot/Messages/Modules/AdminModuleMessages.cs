namespace Grillbot.Messages.Modules
{
    public static class AdminModuleMessages
    {
        public const string CannotFindGuild = "Požadovaný server nebyl nalezen.";

        // GetGuildAsync
        // Embed field names.
        public const string CategoryChannelsCount = "Počet kategorií";
        public const string ChannelsCount = "Počet kanálů";
        public const string CreatedAt = "Vytvořen";
        public const string HasAllMembers = "Uživatelé synchronizováni";
        public const string IsSynced = "Synchronizován";
        public const string MemberCount = "Počet uživatelů (v paměti)";
        public const string RolesCount = "Počet rolí";
        public const string Owner = "Vlastník";
        public const string VerificationLevel = "Úroveň ověření";
        public const string VoiceRegionID = "ID oblasti (Hovory)";
        public const string MfaLevel = "Úroveň MFA";
        public const string ExplicitContentFilter = "Filtr explicitního obsahu";
        public const string SystemChannel = "Systémový kanál";
        public const string DefaultMessageNotifications = "Výchozí notifikace";

        // Attributes
        public const string GetGuildInfoAsyncRemarks = "Parametr guildID je povinný v případě volání v soukromé konverzaci.";
        public const string GetGuildInfoAsyncSummary = "Stav serveru.";
    }
}
