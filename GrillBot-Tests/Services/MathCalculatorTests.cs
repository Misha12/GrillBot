using Grillbot.Services;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot_Tests.Services
{
    [TestClass]
    public class MathCalculatorTests
    {
        [TestMethod]
        public void Solve_MissingExpressionData()
        {
            var options = Options.Create(new Configuration());
            var calculator = new MathService(options);

            var result = calculator.Solve("", null);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void Solve_NotANumber()
        {
            var options = Options.Create(new Configuration());
            var calculator = new MathService(options);

            var result = calculator.Solve("nan", null);

            Assert.IsFalse(result.IsValid);
        }
    }
}
