using System.Collections.Generic;

namespace Grillbot.Services.Config.Models
{
    public class PermissionsConfig
    {
        public List<string> RequiredRoles { get; set; }
        public List<string> AllowedUsers { get; set; }
        public List<string> BannedUsers { get; set; }
        public bool OnlyAdmins { get; set; }

        public PermissionsConfig()
        {
            RequiredRoles = new List<string>();
            AllowedUsers = new List<string>();
            BannedUsers = new List<string>();
        }
    }
}
