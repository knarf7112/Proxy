using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto_UnitTest.CommenUtility
{
    [TestClass]
    public class HexConverter_UnitTest
    {
        private IHexConverter hexConverter;

        [TestInitialize]
        public void InitContext()
        {
            this.hexConverter = new HexConverter(); ;
        }

        [TestMethod]
        public void TestHex2Bytes()
        {
            byte[] expected = { 0x31, 0x32, 0x33, 0x34 };
            string hex = "31323334";
            byte[] result = this.hexConverter.Hex2Bytes(hex);
            //
            Debug.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestHex2Byte()
        {
            byte expected = 0x31;//32(16進位)=>49(10進位)
            string hex = "31";
            byte result = this.hexConverter.Hex2Byte(hex);
            //
            Debug.WriteLine(result);
            Assert.AreEqual(expected, result);
            //
            hex = "3132";
            //Assert.Throws<System.ArgumentOutOfRangeException>
            //(
            //    () => { this.hexConverter.Hex2Byte(hex); }
            //)
            //;
        }

        [TestMethod]
        public void TestByte2Hex()
        {
            byte b = 0x31;
            string expected = "31";
            //
            string result = this.hexConverter.Byte2Hex(b);
            Debug.WriteLine(result);
            Assert.AreEqual(expected, result);
            b = 255;
            result = this.hexConverter.Byte2Hex(b);
            Debug.WriteLine(result);
            expected = "FF";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestBytes2Hex()
        {
            //byte[] bytes = { 0x31, 0x32, 0x33, 0x34 };
            string expected = "31323334";
            //
            string result = this.hexConverter.Bytes2Hex(new byte[] { 0x31, 0x32, 0x33, 0x34 });
            Debug.WriteLine(result);
            Assert.AreEqual(expected, result);
        }

        [TestCleanup]
        public void TearDown()
        {
        }
    }
}
