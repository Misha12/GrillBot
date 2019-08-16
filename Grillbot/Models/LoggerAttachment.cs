using System;

namespace Grillbot.Models
{
    public class LoggerAttachment
    {
        public ulong AttachmentID { get; set; }
        public ulong MessageID { get; set; }
        public string UrlLink { get; set; }
        public string ProxyUrl { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is LoggerAttachment la))
                return false;

            return la.AttachmentID == AttachmentID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AttachmentID);
        }
    }
}
