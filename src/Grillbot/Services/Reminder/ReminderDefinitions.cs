using Discord;
using System.Collections.Generic;

namespace Grillbot.Services.Reminder
{
    public static class ReminderDefinitions
    {
        public static Emoji OneHour => new Emoji("1️⃣");
        public static Emoji TwoHours => new Emoji("2️⃣");
        public static Emoji ThreeHours => new Emoji("3️⃣");
        public static Emoji FourHours => new Emoji("4️⃣");
        public static Emoji FiveHours => new Emoji("5️⃣");
        public static Emoji SixHours => new Emoji("6️⃣");
        public static Emoji SevenHours => new Emoji("7️⃣");
        public static Emoji EightHours => new Emoji("8️⃣");
        public static Emoji NineHours => new Emoji("9️⃣");

        public static Emoji[] AllHourEmojis => new[]
        {
            OneHour, TwoHours, ThreeHours, FourHours, FiveHours, SixHours, SevenHours, EightHours, NineHours
        };

        public static Dictionary<Emoji, int> EmojiToHourNumberMapping => new Dictionary<Emoji, int>()
        {
            { OneHour, 1 },
            { TwoHours, 2 },
            { ThreeHours, 3 },
            { FourHours, 4 },
            { FiveHours, 5 },
            { SixHours, 6 },
            { SevenHours, 7 },
            { EightHours, 8 },
            { NineHours, 9 }
        };

        public static Emoji CopyRemindEmoji => new Emoji("🙋");
    }
}
