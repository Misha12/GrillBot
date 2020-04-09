using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace Grillbot.Database.Entity
{
    [Table("TempUnverify")]
    public class TempUnverifyItem : IDisposable
    {
        [Key]
        [Column]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column]
        [Required]
        [StringLength(30)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        [Column]
        [Required]
        [StringLength(30)]
        public string UserID { get; set; }

        [NotMapped]
        public ulong UserIDSnowflake
        {
            get => Convert.ToUInt64(UserID);
            set => UserID = value.ToString();
        }

        [Column]
        [Required]
        public int TimeFor { get; set; }

        [Column]
        [Required]
        public DateTime StartAt { get; set; } = DateTime.Now;

        [Column]
        [Required]
        [JsonIgnore]
        public string RolesToReturn { get; set; }

        [Column]
        [Required]
        [JsonIgnore]
        public string ChannelOverrides { get; set; }

        [Column]
        [Required]
        public string Reason { get; set; }

        [NotMapped]
        public List<ulong> DeserializedRolesToReturn
        {
            get
            {
                if (string.IsNullOrEmpty(RolesToReturn)) return new List<ulong>();
                return JsonConvert.DeserializeObject<List<ulong>>(RolesToReturn);
            }
            set => RolesToReturn = JsonConvert.SerializeObject(value);
        }

        [NotMapped]
        public List<ChannelOverride> DeserializedChannelOverrides
        {
            get
            {
                if (string.IsNullOrEmpty(ChannelOverrides)) return new List<ChannelOverride>();
                return JsonConvert.DeserializeObject<List<ChannelOverride>>(ChannelOverrides);
            }
            set => ChannelOverrides = JsonConvert.SerializeObject(value);
        }

        public DateTime GetEndDatetime() => StartAt.AddSeconds(TimeFor);

        [NotMapped]
        [JsonIgnore]
        public Timer TimerToEnd { get; set; }

        public void InitTimer(TimerCallback callback)
        {
            var time = Convert.ToInt32((GetEndDatetime() - DateTime.Now).TotalMilliseconds);
            TimerToEnd = new Timer(callback, this, time, Timeout.Infinite);
        }

        public void ReInitTimer(TimerCallback callback)
        {
            TimerToEnd.Dispose();
            InitTimer(callback);
        }

        public void Dispose()
        {
            TimerToEnd?.Dispose();
        }
    }
}