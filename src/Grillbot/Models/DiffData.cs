using Newtonsoft.Json;

namespace Grillbot.Models
{
    public class DiffData<TType>
    {
        [JsonProperty("before")]
        public TType Before { get; set; }

        [JsonProperty("after")]
        public TType After { get; set; }

        public DiffData(TType before, TType after)
        {
            Before = before;
            After = after;
        }

        public DiffData() { }
    }
}
