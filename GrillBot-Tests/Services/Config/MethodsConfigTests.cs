using Grillbot.Exceptions;
using Grillbot.Services.Config.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot_Tests.Services.Config
{
    [TestClass]
    public class MethodsConfigTests
    {
        [TestMethod]
        [ExpectedException(typeof(ConfigException))]
        public void GetPermissions_NotFound_Exception()
        {
            var config = new MethodsConfig();

            config.GetPermissions("ABCD");
        }

        [TestMethod]
        public void GetPermissions_Found()
        {
            var config = new MethodsConfig() { MemeImages = new MemeImagesConfig() { Permissions = new PermissionsConfig() } };
            var permissions = config.GetPermissions("MemeImages");

            Assert.IsNotNull(permissions);
        }
    }
}
