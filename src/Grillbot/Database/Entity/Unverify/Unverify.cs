using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Grillbot.Database.Entity.Users;

namespace Grillbot.Database.Entity.Unverify
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

        public long? SetLogOperationID { get; set; }

        [ForeignKey("SetLogOperationID")]
        public UnverifyLog SetLogOperation { get; set; }

        [NotMapped]
        public List<ulong> DeserializedRoles
        {
            get => JsonConvert.DeserializeObject<List<ulong>>(Roles);
            set => Roles = JsonConvert.SerializeObject(value ?? new List<ulong>());
        }

        public string Channels { get; set; }

        [NotMapped]
        public List<ChannelOverride> DeserializedChannels
        {
            get => JsonConvert.DeserializeObject<List<ChannelOverride>>(Channels);
            set => Channels = JsonConvert.SerializeObject(value ?? new List<ChannelOverride>());
        }
    }
}
