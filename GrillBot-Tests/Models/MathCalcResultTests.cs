using Grillbot.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot_Tests.Models
{
    [TestClass]
    public class MathCalcResultTests
    {
        [TestMethod]
        public void GetMention_WithMention()
        {
            const string expectedMention = "<@123>";
            var calcResult = new MathCalcResult() { Mention = expectedMention };
            var result = calcResult.GetMention();

            Assert.AreEqual(expectedMention, result);
        }

        [TestMethod]
        public void GetMention_NoMention()
        {
            var calcResult = new MathCalcResult();
            var result = calcResult.GetMention();

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }
    }
}
