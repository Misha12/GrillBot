using Grillbot.Services.Config.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot_Tests.Services.Config
{
    [TestClass]
    public class ChannelboardConfigTests
    {
        [TestMethod]
        public void GetTokenValidTime()
        {
            const int minutes = 60;

            var config = new ChannelboardConfig() { WebTokenValidMinutes = minutes };
            var expected = TimeSpan.FromMinutes(minutes);

            Assert.AreEqual(expected, config.GetTokenValidTime());
        }
    }
}
