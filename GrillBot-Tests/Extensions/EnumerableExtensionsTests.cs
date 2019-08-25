using Grillbot.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GrillBot_Tests.Extensions
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void DistinctBy()
        {
            var data = new[]
            {
                new { a = 1, b = 2 },
                new { a = 2, b = 2 },
                new { a = 1, b = 2 }
            };

            var result = data.DistinctBy(o => o.a).ToList();

            Assert.IsFalse(result.SequenceEqual(data));
            Assert.AreNotEqual(data.Length, result.Count);
            Assert.AreEqual(2, result.Count);
        }
    }
}
