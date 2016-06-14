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

        /**********************************************************************************************
         
            ADDITIONAL FAIL-SAFE TESTS
         
         **********************************************************************************************/


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestThrowExceptionWhenpassedNumberLessThan1()
        {
            string romanValue = rcon.convert(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestThrowExceptionWhenpassedNumberGreaterThan3999()
        {
            string romanValue = rcon.convert(4000);
        }

        /*------------------------------------------------------------------------------------------------
         *         THE TEST CASES
         *         
         *         Convert Arabic Number to Roman Numeral
                        Number	Numeral
                            1	I
                            3	III
                            9	IX
                           1066	MLXVI
                           1989	MCMLXXXIX
         *------------------------------------------------------------------------------------------------*/

        [TestMethod]
        public void TestReturnIWhenpassed1()
        {
            string romanValue = rcon.convert(1);
            Assert.AreEqual("I", romanValue, String.Format("Expected: I \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnIIIWhenpassed3()
        {
            string romanValue = rcon.convert(3);
            Assert.AreEqual("III", romanValue, String.Format("Expected: III \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnIXWhenpassed9()
        {
            string romanValue = rcon.convert(9);
            Assert.AreEqual("IX", romanValue, String.Format("Expected: IX \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnMLXVIWhenpassed1066()
        {
            string romanValue = rcon.convert(1066);
            Assert.AreEqual("MLXVI", romanValue, String.Format("Expected: MLXVI \n Returned: {0}", romanValue));
        }

        [TestMethod]
        public void TestReturnMCMLXXXIXWhenpassed1989()
        {
            string romanValue = rcon.convert(1989);
            Assert.AreEqual("MCMLXXXIX", romanValue, String.Format("Expected: MCMLXXXIX \n Returned: {0}", romanValue));
        }
    }
}
