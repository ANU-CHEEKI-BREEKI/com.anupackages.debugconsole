#if USE_NCALC
using System.Linq;
using NUnit.Framework;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public class ExpressionEvaluation : TestBase
    {
        [Test]
        public void Echo_Expression_Simple()
        {
            var result = Context.ExecuteCommand("echo 1+2*3");
            Assert.AreEqual("7", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Evaluate_Expression()
        {
            var result = Context.ExecuteCommand("$evaluate 1+2*3");
            Assert.AreEqual(7, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Evaluate_NestedDefines()
        {
            Context.ExecuteCommand("#define a 1+2");
            Context.ExecuteCommand("#define b #a*#a");
            var result = Context.ExecuteCommand("echo 1+#b");
            Assert.AreEqual("10", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Defines_Echo()
        {
            Context.ExecuteCommand("#define a 1+2");
            var result = Context.ExecuteCommand("#echo #a");
            Assert.AreEqual("1+2", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Defines_Evaluate()
        {
            Context.ExecuteCommand("#define a 1+2");
            var result = Context.ExecuteCommand("echo #a");
            Assert.AreEqual("3", result.ReturnValues.FirstOrDefault().ReturnValue);
        }
    }
}
#endif