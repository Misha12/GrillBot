using System.ComponentModel.DataAnnotations;

namespace Grillbot.Database.Entity
{
    public class File
    {
        [Key]
        public string Filename { get; set; }

        public byte[] Content { get; set; }
    }
}
