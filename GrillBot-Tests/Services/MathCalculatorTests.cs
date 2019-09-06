using Grillbot.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
