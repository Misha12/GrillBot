namespace Grillbot.Enums
{
    public enum LeaderboardErrors
    {
        Unknown,

        /// <summary>
        /// Success, data returned.
        /// </summary>
        Success,

        /// <summary>
        /// Invalid access token.
        /// </summary>
        InvalidKey,

        /// <summary>
        /// Guild not exists or not available.
        /// </summary>
        InvalidGuild,

        /// <summary>
        /// User is not in guild.
        /// </summary>
        UserAtGuildNotFound
    }
}
