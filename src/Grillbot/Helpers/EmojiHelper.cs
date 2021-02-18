using Discord;
using System;
using System.Collections.Generic;

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
        public static Emoji LetterA => new Emoji("🇦");
        public static Emoji LetterB => new Emoji("🇧");
        public static Emoji LetterC => new Emoji("🇨");
        public static Emoji LetterD => new Emoji("🇩");
        public static Emoji LetterE => new Emoji("🇪");
        public static Emoji LetterF => new Emoji("🇫");
        public static Emoji LetterG => new Emoji("🇬");
        public static Emoji LetterH => new Emoji("🇭");
        public static Emoji LetterI => new Emoji("🇮");
        public static Emoji LetterJ => new Emoji("🇯");
        public static Emoji LetterK => new Emoji("🇰");
        public static Emoji LetterL => new Emoji("🇱");
        public static Emoji LetterM => new Emoji("🇲");
        public static Emoji LetterN => new Emoji("🇳");
        public static Emoji LetterO => new Emoji("🇴");
        public static Emoji LetterP => new Emoji("🇵");
        public static Emoji LetterQ => new Emoji("🇶");
        public static Emoji LetterR => new Emoji("🇷");
        public static Emoji LetterS => new Emoji("🇸");
        public static Emoji LetterT => new Emoji("🇹");
        public static Emoji LetterU => new Emoji("🇺");
        public static Emoji LetterV => new Emoji("🇻");
        public static Emoji LetterW => new Emoji("🇼");
        public static Emoji LetterX => new Emoji("🇽");
        public static Emoji LetterY => new Emoji("🇾");
        public static Emoji LetterZ => new Emoji("🇿");
        public static Emoji SignA => new Emoji("🅰️");
        public static Emoji SignB => new Emoji("🅱️");
        public static Emoji SignO => new Emoji("🅾️");
        public static Emoji One => new Emoji("1️⃣");
        public static Emoji Two => new Emoji("2️⃣");
        public static Emoji Three => new Emoji("3️⃣");
        public static Emoji Four => new Emoji("4️⃣");
        public static Emoji Five => new Emoji("5️⃣");
        public static Emoji Six => new Emoji("6️⃣");
        public static Emoji Seven => new Emoji("7️⃣");
        public static Emoji Eight => new Emoji("8️⃣");
        public static Emoji Nine => new Emoji("9️⃣");
        public static Emoji PersonRisingHand => new Emoji("🙋");

        public static Dictionary<int, Emoji> NumberToEmojiMap => new Dictionary<int, Emoji>()
        {
            { 1, One }, { 2, Two }, { 3, Three }, { 4, Four }, { 5, Five }, { 6, Six }, { 7, Seven }, { 8, Eight }, { 9, Nine }
        };

        public static Dictionary<Emoji, int> EmojiToIntMap => new Dictionary<Emoji, int>()
        {
            { One, 1 }, { Two, 2}, { Three, 3 }, { Four, 4 }, { Five, 5 }, { Six, 6 }, { Seven, 7}, { Eight, 8 }, { Nine, 9 }
        };

        public static Dictionary<char, Emoji> CharToEmojiMap => new Dictionary<char, Emoji>()
        {
            { 'A', LetterA },
            { 'B', LetterB },
            { 'C', LetterC },
            { 'D', LetterD },
            { 'E', LetterE },
            { 'F', LetterF },
            { 'G', LetterG },
            { 'H', LetterH },
            { 'I', LetterI },
            { 'J', LetterJ },
            { 'K', LetterK },
            { 'L', LetterL },
            { 'M', LetterM },
            { 'N', LetterN },
            { 'O', LetterO },
            { 'P', LetterP },
            { 'Q', LetterQ },
            { 'R', LetterR },
            { 'S', LetterS },
            { 'T', LetterT },
            { 'U', LetterU },
            { 'V', LetterV },
            { 'W', LetterW },
            { 'X', LetterX },
            { 'Y', LetterY },
            { 'Z', LetterZ }
        };

        public static Dictionary<char, Emoji> CharToSignEmojiMap => new Dictionary<char, Emoji>()
        {
            { 'A', SignA },
            { 'B', SignB },
            { 'O', SignO }
        };

        public static List<Emoji> ConvertStringToEmoji(string str, bool allowDuplicity = false)
        {
            str = str.ToUpper();

            var result = new List<Emoji>();
            foreach (var character in str)
            {
                var emoji = ConvertCharacterToEmoji(character);

                if (result.Contains(emoji) && !allowDuplicity)
                    emoji = ConvertCharacterToEmoji(character, true);

                if (result.Contains(emoji) && !allowDuplicity)
                    throw new ArgumentException($"Duplicitní znak `{character}`.");

                result.Add(emoji);
            }

            return result;
        }

        public static Emoji ConvertCharacterToEmoji(char character, bool alternativeFirst = false)
        {
            if (char.IsDigit(character) && NumberToEmojiMap.ContainsKey((int)char.GetNumericValue(character)))
                return NumberToEmojiMap[(int)char.GetNumericValue(character)];

            if (alternativeFirst && CharToSignEmojiMap.ContainsKey(character))
                return CharToSignEmojiMap[character];

            return CharToEmojiMap.ContainsKey(character) ? CharToEmojiMap[character] : null;
        }
    }
}
