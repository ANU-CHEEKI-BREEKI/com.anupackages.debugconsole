using System.Linq;
using NUnit.Framework;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public class NestedCommands : TestBase
    {
        public override void SetUpOnce()
        {
            base.SetUpOnce();

            Context.Commands.RegisterCommand<int>("get-int", "", () => 125);
            Context.Commands.RegisterCommand<int, int>("mul-2-int", "", v => v * 2);
        }

        [Test]
        public void NestedCommand_1()
        {
            var result = Context.ExecuteCommand("echo {get-int}");
            Assert.AreEqual("125", result.ReturnValues.First().ReturnValue);
        }
       
        [Test]
        public void NestedCommand_2()
        {
            var result = Context.ExecuteCommand("echo {mul-2-int {get-int}}");
            Assert.AreEqual("250", result.ReturnValues.First().ReturnValue);
        }
    }
}
