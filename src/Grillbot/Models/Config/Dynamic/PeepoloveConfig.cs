using Newtonsoft.Json;
using System.Drawing;
using System.IO;

namespace Grillbot.Models.Config.Dynamic
{
    public class PeepoloveConfig
    {
        public string BasePath { get; set; }
        public string BodyFilename { get; set; }
        public string HandsFilename { get; set; }
        public Rectangle Screen { get; set; }
        public float Rotate { get; set; }
        public Rectangle ProfilePicRect { get; set; }
        public Rectangle CropRect { get; set; }
        public ushort ProfilePicSize { get; set; }

        [JsonIgnore]
        public string BodyPath => Path.Combine(BasePath, BodyFilename);

        [JsonIgnore]
        public string HandsPath => Path.Combine(BasePath, HandsFilename);
    }
}
