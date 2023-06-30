using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    [TestFixture]
    public class DebugConsoleCommandsTests
    {
        private DebugConsoleProcessor _processor;

        [OneTimeSetUp]
        public void Initialize()
        {
            _processor = new DebugConsoleProcessor();
            _processor.Initialize();
            
            var init = new AttributeCommandsInitializerProcessor(
                _processor.Logger,
                _processor.Commands
            );
            init.Initialize();
        }

        [Test]
        public void DebugConsoleCommandsTestsSimplePasses()
        {
            var result = _processor.ExecuteCommand("echo lol");
        }
    }
}
