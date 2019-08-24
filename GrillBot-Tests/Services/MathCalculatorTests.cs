using Discord.WebSocket;
using Grillbot.Services;
using GrillBot_Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace GrillBot_Tests.Services
{
    [TestClass]
    public class MathCalculatorTests
    {
        [TestMethod]
        public void Solve_MissingExpressionData()
        {
            var calculator = new MathCalculator(null);

            var result = calculator.Solve("", null);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void Solve_NotANumber()
        {
            var calculator = new MathCalculator(null);

            var result = calculator.Solve("nan", null);

            Assert.IsFalse(result.IsValid);
        }
    }
}
