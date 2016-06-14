using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RomanConverter.Test
{
    [TestClass]
    public class RomanConverterTest
    {
        RomanConverter rcon = new RomanConverter();

        /**********************************************************************************************
         
            THE INITIAL SETUP TESTS
         
         **********************************************************************************************/


        [TestMethod]
        public void TestReturnIWhenpassed1()
        {
            string romanValue = rcon.convert(1);
            Assert.AreEqual("I", romanValue, String.Format("Expected: I \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnVWhenpassed5()
        {
            string romanValue = rcon.convert(5);
            Assert.AreEqual("V", romanValue, String.Format("Expected: V \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnXWhenpassed10()
        {
            string romanValue = rcon.convert(10);
            Assert.AreEqual("X", romanValue, String.Format("Expected: X \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnLWhenpassed50()
        {
            string romanValue = rcon.convert(50);
            Assert.AreEqual("L", romanValue, String.Format("Expected: L \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnCWhenpassed100()
        {
            string romanValue = rcon.convert(100);
            Assert.AreEqual("C", romanValue, String.Format("Expected: C \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnDWhenpassed500()
        {
            string romanValue = rcon.convert(500);
            Assert.AreEqual("D", romanValue, String.Format("Expected: D \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnMWhenpassed1000()
        {
            string romanValue = rcon.convert(1000);
            Assert.AreEqual("M", romanValue, String.Format("Expected: M \n Returned: {0}", romanValue));
        }

    }
}
