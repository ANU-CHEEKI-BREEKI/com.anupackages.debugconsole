using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public void SetUpOnce()
        {
            _processor = new DebugConsoleProcessor();
            _processor.Initialize();

            var init = new AttributeCommandsInitializerProcessor(
                _processor.Logger,
                _processor.Commands
            );
            init.Initialize();

            _processor.Commands.RegisterCommand<Vector3, Vector3>("echo-vec3", "", v => v);
            _processor.Commands.RegisterCommand<bool, bool>("echo-bool", "", v => v);
        }

        [SetUp]
        public void SetUp()
        {
            _processor.Defines.Clear();
        }

        [Test]
        public void Echo_Strgin()
        {
            var result = _processor.ExecuteCommand("echo lol");
            Assert.AreEqual("lol", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Echo_Expression_Simple()
        {
            var result = _processor.ExecuteCommand("echo 1+2*3");
            Assert.AreEqual("7", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Evaluate_Expression()
        {
            var result = _processor.ExecuteCommand("$evaluate 1+2*3");
            Assert.AreEqual(7, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Evaluate_NestedDefines()
        {
            _processor.ExecuteCommand("#define a 1+2");
            _processor.ExecuteCommand("#define b #a*#a");
            var result = _processor.ExecuteCommand("echo 1+#b");
            Assert.AreEqual("10", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Defines_Echo()
        {
            _processor.ExecuteCommand("#define a 1+2");
            var result = _processor.ExecuteCommand("#echo #a");
            Assert.AreEqual("1+2", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Defines_Evaluate()
        {
            _processor.ExecuteCommand("#define a 1+2");
            var result = _processor.ExecuteCommand("echo #a");
            Assert.AreEqual("3", result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_BracketsSquare()
        {
            var result = _processor.ExecuteCommand("echo-vec3 [1,2,3]");
            Assert.AreEqual(new Vector3(1, 2, 3), result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_BracketsRound()
        {
            var result = _processor.ExecuteCommand("echo-vec3 (1,2,3)");
            Assert.AreEqual(new Vector3(1, 2, 3), result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_BracketsSquare_WSpaces()
        {
            var result = _processor.ExecuteCommand("echo-vec3 [1 ,2 ,  3]");
            Assert.AreEqual(new Vector3(1, 2, 3), result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_BracketsRound_WSpaces()
        {
            var result = _processor.ExecuteCommand("echo-vec3 (1 , 2 , 3)");
            Assert.AreEqual(new Vector3(1, 2, 3), result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_Zero()
        {
            var result = _processor.ExecuteCommand("echo-vec3 []");
            Assert.AreEqual(Vector3.zero, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Vector_3_N()
        {
            var result = _processor.ExecuteCommand("echo-vec3 [3]");
            Assert.AreEqual(Vector3.one * 3, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Bool_T()
        {
            var result = _processor.ExecuteCommand("echo-bool 1");
            Assert.AreEqual(true, result.ReturnValues.FirstOrDefault().ReturnValue);
        }

        [Test]
        public void Bool_F()
        {
            var result = _processor.ExecuteCommand("echo-bool no");
            Assert.AreEqual(false, result.ReturnValues.FirstOrDefault().ReturnValue);
        }
    }
}
