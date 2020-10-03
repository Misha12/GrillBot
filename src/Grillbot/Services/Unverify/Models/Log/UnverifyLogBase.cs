using Newtonsoft.Json.Linq;

namespace Grillbot.Services.Unverify.Models.Log
{
    public abstract class UnverifyLogBase
    {
        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}
