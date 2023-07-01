using System.Linq;
using NUnit.Framework;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public class Defines : TestBase
    {
        [Test]
        public void Evaluate_NestedDefines()
        {
            Context.ExecuteCommand("#define a \"Hello! And welcome to the los pollos hermanos family\"");
            Context.ExecuteCommand("#define b \"my name is Gustavo\"");
            Context.ExecuteCommand("#define c \"but you can call me Gus\"");
            Context.ExecuteCommand("#define d \"#a, #b\"");
            Context.ExecuteCommand("#define e \"#d, #c\"");

            var result = Context.ExecuteCommand("echo #e");
            var expected = "Hello! And welcome to the los pollos hermanos family, my name is Gustavo, but you can call me Gus";

            Assert.AreEqual(expected, result.ReturnValues.FirstOrDefault().ReturnValue);
        }
    }
}
