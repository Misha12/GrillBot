using Discord;

namespace Grillbot.Services.Logger.LoggerMethods.LogEmbed
{
    public enum LogEmbedType
    {
        MessageDeleted,
        MessageEdited,
        UserJoined,
        UserLeft,
        UserUpdated,
        GuildMemberUpdated,
        BoostUpdated
    }

    public static class LogEmbedTypeExtensions
    {
        public static Color GetColor(this LogEmbedType type)
        {
            switch (type)
            {
                case LogEmbedType.MessageDeleted: return Color.Red;
                case LogEmbedType.MessageEdited: return new Color(255, 255, 0);
                case LogEmbedType.UserJoined:
                case LogEmbedType.UserLeft:
                case LogEmbedType.UserUpdated:
                    return Color.Green;
                case LogEmbedType.GuildMemberUpdated: return Color.DarkBlue;
                case LogEmbedType.BoostUpdated:
                    return new Color(255, 0, 207);
            }

            return Color.Blue;
        }
    }
}
