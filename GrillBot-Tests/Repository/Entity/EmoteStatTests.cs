using Grillbot.Repository.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrillBot_Tests.Repository.Entity
{
    [TestClass]
    public class EmoteStatTests
    {
        [TestMethod]
        public void ConstructorIncrement()
        {
            const string emote = "<:rtzW:123456>";
            var result = new EmoteStat(emote);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(emote, result.EmoteID);
        }

        [TestMethod]
        public void GetFormatedInfo()
        {
            const string emote = "<:rtzW:123456>";
            var emoteStat = new EmoteStat(emote);

            var result = emoteStat.GetFormatedInfo();

            Assert.IsFalse(string.IsNullOrEmpty(result));
        }
    }
}
