using NUnit.Framework;
using UnityEngine;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        protected DebugConsoleProcessor Context { get; private set; }

        [OneTimeSetUp]
        public virtual void SetUpOnce()
        {
            Context = new DebugConsoleProcessor();
            Context.Initialize();

            var init = new AttributeCommandsInitializerProcessor(
                Context.Logger,
                Context.Commands
            );
            init.Initialize();
        }

        [SetUp]
        public void SetUp() => Context.Defines.Clear();
    }
}
