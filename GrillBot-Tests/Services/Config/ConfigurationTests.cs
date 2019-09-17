using Grillbot.Services.Config.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot_Tests.Services.Config
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void IsUserBotAdmin_True() => IsUserBotAdmin_Test(12345, true);

        [TestMethod]
        public void IsUserBotAdmin_False() => IsUserBotAdmin_Test(123456, false);

        private void IsUserBotAdmin_Test(ulong value, bool expected)
        {
            var configuration = new Configuration();
            configuration.Administrators.Add("12345");

            var result = configuration.IsUserBotAdmin(value);

            Assert.AreEqual(expected, result);
        }
    }
}
