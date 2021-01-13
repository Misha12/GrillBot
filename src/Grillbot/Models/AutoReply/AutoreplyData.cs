using Discord.WebSocket;
using Grillbot.Database.Entity;
using System;
using System.Text.RegularExpressions;

namespace Grillbot.Models.AutoReply
{
    public class AutoreplyData
    {
        public string MustContains { get; set; }
        public string ReplyMessage { get; set; }
        public AutoReplyCompareTypes CompareType { get; set; }
        public int Flags { get; set; }
        public string Channel { get; set; }

        public bool AllChannels => Channel == "*";

        public static AutoreplyData Parse(string data)
        {
            var match = Regex.Match(data, @"^```([^`]*)```[\n]*?```([^`]*)```\n(==|Contains)\n(\d*)\n(\*|\d*)");

            if (!match.Success)
                throw new ArgumentException("Nebyl dodržen správný formát vstupu automatické odpovědi.");

            if (string.IsNullOrEmpty(match.Groups[1].Value.Trim()))
                throw new ArgumentException("Nebyl zadán povinný text vyhledávané zprávy.");

            if (string.IsNullOrEmpty(match.Groups[2].Value.Trim()))
                throw new ArgumentException("Nebyl zadán povinný text odpovědi.");

            var channel = match.Groups[5].Value.Trim();
            if (string.IsNullOrEmpty(channel))
                throw new ArgumentException("Nebyl zadán výstupní kanál.");

            if (channel != "*" && !ulong.TryParse(channel, out ulong _))
                throw new ArgumentException("Nebyla zadána správná identifikace výstupního kanálu.");

            var result = new AutoreplyData()
            {
                MustContains = match.Groups[1].Value.Trim(),
                ReplyMessage = match.Groups[2].Value.Trim(),
                Flags = string.IsNullOrEmpty(match.Groups[4].Value.Trim()) ? 0 : Convert.ToInt32(match.Groups[4].Value.Trim()),
                Channel = channel
            };

            switch (match.Groups[3].Value.Trim().ToLower())
            {
                case "==":
                    result.CompareType = AutoReplyCompareTypes.Absolute;
                    break;
                case "contains":
                    result.CompareType = AutoReplyCompareTypes.Contains;
                    break;
            }

            return result;
        }

        public Database.Entity.AutoReplyItem ToEntity(SocketGuild guild)
        {
            return new Database.Entity.AutoReplyItem()
            {
                MustContains = MustContains,
                ChannelIDSnowflake = AllChannels ? null : Convert.ToUInt64(Channel),
                CompareType = CompareType,
                GuildIDSnowflake = guild.Id,
                ReplyMessage = ReplyMessage,
                Flags = Flags
            };
        }

        public void UpdateEntity(Database.Entity.AutoReplyItem entity)
        {
            entity.MustContains = MustContains;
            entity.ChannelIDSnowflake = AllChannels ? null : Convert.ToUInt64(Channel);
            entity.CompareType = CompareType;
            entity.Flags = Flags;
            entity.ReplyMessage = ReplyMessage;
        }
    }
}
