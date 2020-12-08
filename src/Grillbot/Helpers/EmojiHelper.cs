using Discord;

namespace Grillbot.Helpers
{
    public static class EmojiHelper
    {
        public static Emoji OKEmoji => new Emoji("✅");
        public static Emoji NOKEmoji => new Emoji("❌");
        public static Emoji TrackPrevious => new Emoji("⏮️");
        public static Emoji ArrowBackward => new Emoji("◀️");
        public static Emoji ArrowForward => new Emoji("▶️");
        public static Emoji TrackNext => new Emoji("⏭️");
    }
}
