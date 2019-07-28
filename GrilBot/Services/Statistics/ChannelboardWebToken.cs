using System;

namespace GrilBot.Services.Statistics
{
    public class ChannelboardWebToken
    {
        public string Token { get; set; }
        public ulong UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidToUtc { get; set; }
        public string Url { get; set; }

        public bool IsValid() => DateTime.UtcNow < ValidToUtc;

        public ChannelboardWebToken(string token, ulong userID, TimeSpan validFor, string rawUrl)
        {
            CreatedAt = DateTime.UtcNow;

            Token = token;
            UserID = userID;
            ValidToUtc = CreatedAt.Add(validFor);
            Url = string.Format(rawUrl, token);
        }

        public DateTime GetExpirationDate() => ValidToUtc.ToLocalTime();
    }
}
