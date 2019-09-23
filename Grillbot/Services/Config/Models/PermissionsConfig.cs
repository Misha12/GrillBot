using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public bool IsUserAllowed(ulong userID) => AllowedUsers.Contains(userID.ToString());
        public bool IsUserBanned(ulong userID) => BannedUsers.Contains(userID.ToString());
        public bool IsRoleAllowed(string roleName) => RequiredRoles.Contains(roleName);

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder
                .Append("RequiredRoles: [").Append(string.Join(", ", RequiredRoles)).Append("],")
                .Append("AllowedUsers: [").Append(string.Join(", ", AllowedUsers)).Append("], ")
                .Append("BannedUsers: [").Append(string.Join(", ", BannedUsers)).Append("], ")
                .Append("OnlyAdmins: ").Append(OnlyAdmins);

            return builder.ToString();
        }
    }
}
