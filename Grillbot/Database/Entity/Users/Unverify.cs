using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.Users
{
    [Table("Unverifies")]
    public class Unverify
    {
        [Key]
        public long UserID { get; set; }

        [ForeignKey("UserID")]
        public DiscordUser User { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Reason { get; set; }
        public string Roles { get; set; }

        [NotMapped]
        public List<ulong> DeserializedRoles
        {
            get => JsonConvert.DeserializeObject<List<ulong>>(Roles);
            set => JsonConvert.SerializeObject(value ?? new List<ulong>());
        }

        public string Channels { get; set; }

        [NotMapped]
        public List<ChannelOverride> DeserializedChannels
        {
            get => JsonConvert.DeserializeObject<List<ChannelOverride>>(Channels);
            set => JsonConvert.SerializeObject(value ?? new List<ChannelOverride>());
        }
    }
}
