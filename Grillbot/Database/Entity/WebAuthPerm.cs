using Grillbot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    [Table("WebAuthPerm")]
    public class WebAuthPerm
    {
        /// <summary>
        /// User or role ID
        /// </summary>
        [Column]
        [StringLength(30)]
        public string ID { get; set; }

        public string Password { get; set; }

        [StringLength(30)]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong IDSnowflake
        {
            get => Convert.ToUInt64(ID);
            set => ID = value.ToString();
        }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        public bool IsValidPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, Password);
        }
    }
}
