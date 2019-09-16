using Grillbot.Exceptions;

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

        public PermissionsConfig GetPermissions(string section)
        {
            var property = GetType().GetProperty(section);

            if (property == null)
                throw new ConfigException($"Section {section} not found in config");

            return ((MethodConfigBase)property.GetValue(this, null))?.Permissions;
        }
    }
}
