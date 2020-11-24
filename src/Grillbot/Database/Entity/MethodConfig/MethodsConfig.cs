using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity.MethodConfig
{
    [Table("MethodsConfig")]
    public class MethodsConfig
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
        [JsonIgnore]
        public ulong GuildIDSnowflake => Convert.ToUInt64(GuildID);

        [Column]
        [Required]
        [StringLength(100)]
        public string Group { get; set; }

        [Column]
        [Required]
        [StringLength(100)]
        public string Command { get; set; }

        [Column]
        [Required]
        [JsonIgnore]
        public string ConfigData { get; set; }

        [NotMapped]
        public JObject Config
        {
            get => string.IsNullOrEmpty(ConfigData) ? null : JObject.Parse(ConfigData);
            set => ConfigData = (value ?? new JObject()).ToString(Formatting.None);
        }

        public TData GetData<TData>() => Config == null ? default : Config.ToObject<TData>();

        [Column]
        public bool OnlyAdmins { get; set; }

        [Column]
        public long UsedCount { get; set; }

        public ISet<MethodPerm> Permissions { get; set; }

        public MethodsConfig()
        {
            Permissions = new HashSet<MethodPerm>();
        }

        public static MethodsConfig Create(IGuild guild, string group, string command, bool onlyAdmins, JObject json)
        {
            return Create(guild.Id, group, command, onlyAdmins, json);
        }

        public static MethodsConfig Create(ulong guildID, string group, string command, bool onlyAdmins, JObject json)
        {
            return new MethodsConfig()
            {
                Command = string.IsNullOrEmpty(command) ? "" : command,
                Config = json,
                Group = string.IsNullOrEmpty(group) ? "" : group,
                GuildID = guildID.ToString(),
                OnlyAdmins = onlyAdmins
            };
        }

        public override string ToString()
        {
            return $"{GuildID}/{Group}/{Command}";
        }
    }
}
