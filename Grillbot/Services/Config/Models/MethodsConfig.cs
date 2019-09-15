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
    }
}
