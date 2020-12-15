using Discord;
using Newtonsoft.Json;

namespace Grillbot.Models.Audit
{
    public class OverwritePermissionsValue
    {
        [JsonProperty("allow")]
        public ulong AllowValue { get; set; }

        [JsonProperty("deny")]
        public ulong DenyValue { get; set; }

        [JsonIgnore]
        public OverwritePermissions Permissions => new OverwritePermissions(AllowValue, DenyValue);

        public OverwritePermissionsValue(OverwritePermissions perms)
        {
            AllowValue = perms.AllowValue;
            DenyValue = perms.DenyValue;
        }

        public OverwritePermissionsValue() { }

        public override bool Equals(object obj)
        {
            if (obj is not OverwritePermissionsValue perms)
                return false;

            return perms.AllowValue == AllowValue && perms.DenyValue == DenyValue;
        }

        public override int GetHashCode()
        {
            return (AllowValue + DenyValue).ToString().GetHashCode();
        }
    }
}
