using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CALib;
using CAAssembler;
using CAEmulator;
using System.IO;

namespace Tests
{
    [TestClass]
    public class AssemblerTests
    {
        [TestMethod]
        public void OpCodesGreaterThanZero()
        {
            foreach (string s in Enum.GetNames(typeof(OpCodes)))
            {
                OpCodes code = (OpCodes)Enum.Parse(typeof(OpCodes), s);
                Assert.IsTrue((int)code >= 0, $"OpCode {s} had a value of {(int)code:X}, which is less than the minimum allowed of 00.");
            }
        }
        [TestMethod]
        public void OpCodesLessThanOrEqualTo255()
        {
            foreach (string s in Enum.GetNames(typeof(OpCodes)))
            {
                OpCodes code = (OpCodes)Enum.Parse(typeof(OpCodes), s);
                Assert.IsTrue((int)code <= 255, $"OpCode {s} had a value of {(int)code:X}, which is more than the maximum allowed of FF.");
            }
        }
        [TestMethod]
        public void AssemblesCorrectly()
        {
            //CAAssembler.Program.Assemble();
            //Assert.AreEqual();
        }
    }
}

