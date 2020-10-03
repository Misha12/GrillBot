using Grillbot.Extensions;
using System;
using System.Text;

namespace Grillbot.Models.EmoteStats
{
    public class GroupedEmoteItem
    {
        public string EmoteID { get; set; }
        public long UseCount { get; set; }
        public DateTime LastOccuredAt { get; set; } = DateTime.Now;
        public DateTime FirstOccuredAt { get; set; } = DateTime.Now;
        public bool IsUnicode { get; set; }
        public int UsersCount { get; set; }

        public string RealID
        {
            get => IsUnicode ? Encoding.Unicode.GetString(Convert.FromBase64String(EmoteID)) : EmoteID;
        }

        public string GetFormatedInfo(bool noUserCount = false)
        {
            var builder = new StringBuilder()
                .Append("Počet použití: ").AppendLine(UseCount.FormatWithSpaces())
                .Append("Poprvé použito: ").AppendLine(FirstOccuredAt.ToLocaleDatetime())
                .Append("Naposledy použito: ").AppendLine(LastOccuredAt.ToLocaleDatetime());

            if (!noUserCount)
                builder.Append("Použilo uživatelů: ").AppendLine(UsersCount.FormatWithSpaces());

            return builder.ToString();
        }
    }
}
