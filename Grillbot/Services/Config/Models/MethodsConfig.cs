using Grillbot.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Config.Models
{
    public class MethodsConfig
    {
        public GreetingConfig Greeting { get; set; }
        public GrillStatusConfig GrillStatus { get; set; }
        public HelpConfig Help { get; set; }
        public ChannelboardConfig Channelboard { get; set; }
        public MemeImagesConfig MemeImages { get; set; }
        public RoleManagerConfig RoleManager { get; set; }
        public MathConfig Math { get; set; }
        public AutoReplyConfig AutoReply { get; set; }
        public TeamSearchConfig TeamSearch { get; set; }
        public EmoteManagerConfig EmoteManager { get; set; }
        public ModifyConfigConfig ModifyConfig { get; set; }
        public TempUnverifyConfig TempUnverify { get; set; }
        public AdminConfig Admin { get; set; }
        public SelfUnverifyConfig SelfUnverify { get; set; }
        public CReferenceConfig CReference { get; set; }
        public MemesConfig Memes { get; set; }

        public PermissionsConfig GetPermissions(string section)
        {
            var property = GetType().GetProperty(section);

            if (property == null)
                throw new ConfigException($"Section {section} not found in config");

            return ((MethodConfigBase)property.GetValue(this, null))?.Permissions;
        }

        public void SetPermissions(string section, string type, string value)
        {
            var permissions = GetPermissions(section);

            switch(type)
            {
                case "AddUser":
                    permissions.AllowedUsers.Add(value);
                    break;
                case "AddRole":
                    permissions.RequiredRoles.Add(value);
                    break;
                case "RemoveUser":
                    permissions.AllowedUsers.Remove(value);
                    break;
                case "BanUser":
                    permissions.BannedUsers.Add(value);
                    break;
                case "UnbanUser":
                    permissions.BannedUsers.Remove(value);
                    break;
                case "RemoveRole":
                    permissions.RequiredRoles.Remove(value);
                    break;
                case "OnlyAdmins":
                    if(bool.TryParse(value, out bool result))
                    {
                        permissions.OnlyAdmins = result;
                    }
                    break;
            }
        }

        public List<string> GetPermissionNames()
        {
            return GetType()
                .GetProperties()
                .Select(o => $"- {o.Name} - {GetPermissions(o.Name)?.ToString() ?? "**Missing**"}")
                .ToList();
        }
    }
}
