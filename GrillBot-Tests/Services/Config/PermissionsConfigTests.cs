using Grillbot.Services.Config.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot_Tests.Services.Config
{
    [TestClass]
    public class PermissionsConfigTests
    {
        [TestMethod]
        public void IsUserAllowed_True() => IsUserAllowed_Test(123456, true);

        [TestMethod]
        public void IsUserAllowed_False() => IsUserAllowed_Test(12345, false);

        public void IsUserAllowed_Test(ulong id, bool expected)
        {
            var config = new PermissionsConfig();
            config.AllowedUsers.Add("123456");

            var result = config.IsUserAllowed(id);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IsUserBanned_True() => IsUserBanned_Test(123456, true);

        [TestMethod]
        public void IsUserBanned_False() => IsUserBanned_Test(13456, false);

        public void IsUserBanned_Test(ulong id, bool expected)
        {
            var config = new PermissionsConfig();
            config.BannedUsers.Add("123456");

            var result = config.IsUserBanned(id);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void IsRoleAllowed_True() => IsRoleAllowed_Test("Verify", true);

        [TestMethod]
        public void IsRoleAllowed_False() => IsRoleAllowed_Test("Mod", false);

        public void IsRoleAllowed_Test(string role, bool expected)
        {
            var config = new PermissionsConfig();
            config.RequiredRoles.Add("Verify");

            var result = config.IsRoleAllowed(role);

            Assert.AreEqual(expected, result);
        }
    }
}
