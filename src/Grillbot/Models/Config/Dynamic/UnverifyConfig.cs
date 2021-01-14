namespace Grillbot.Models.Config.Dynamic
{
    public class UnverifyConfig
    {
        public ulong MutedRoleID { get; set; }
        public int CooldownHours { get; set; } = 6;
    }
}
