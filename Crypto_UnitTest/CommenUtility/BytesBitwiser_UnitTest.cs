using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto_UnitTest.CommenUtility
{
    [TestClass]
    public class BytesBitwiser_UnitTest
    {
        private IHexConverter hexConverter;
        private IBytesBitwiser bytesBitwiser;
        private byte[] allZero;
        private byte[] constRb;
        private byte[] k0;

        [TestInitialize]
        public void Init()
        {
            hexConverter = new HexConverter();
            bytesBitwiser = new BytesBitwiser();

            this.allZero = new byte[16];
            for (int i = 0; i < allZero.Length; i++)
            {
                allZero[i] = 0;
            }
            this.constRb = new byte[16];
            Array.Copy(allZero, constRb, allZero.Length - 1);
            constRb[constRb.Length - 1] = 0x87;
            //
            this.k0 = this.hexConverter.Hex2Bytes("52DB5AFE7B64EFFAB1E92EEA983C5F73");
        }

        [TestMethod]
        public void Test_ShiftLeft()
        {
            string expected = "A5B6B5FCF6C9DFF563D25DD53078BEE6";
            byte[] k1;

            if (!this.bytesBitwiser.MsbOne(this.k0))
            {
                k1 = this.bytesBitwiser.ShiftLeft(this.k0, 1);
            }
            else
            {
                k1 = this.bytesBitwiser.ExclusiveOr(this.bytesBitwiser.ShiftLeft(this.k0, 1), this.constRb);
            }
            string result = this.hexConverter.Bytes2Hex(k1);
            Debug.WriteLine("Expect:\t" + expected);
            Debug.WriteLine("Result:\t" + result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_ExclusiveOr()
        {
            byte[] k1, k2;
            if (!this.bytesBitwiser.MsbOne(this.k0))
            {
                k1 = this.bytesBitwiser.ShiftLeft(this.k0, 1);
            }
            else
            {
                k1 = this.bytesBitwiser.ExclusiveOr(this.bytesBitwiser.ShiftLeft(this.k0, 1), this.constRb);
            }
            //
            string expected = "4B6D6BF9ED93BFEAC7A4BBAA60F17D4B";
            if (!this.bytesBitwiser.MsbOne(k1))
            {
                k2 = this.bytesBitwiser.ShiftLeft(k1, 1);
            }
            else
            {
                k2 = this.bytesBitwiser.ExclusiveOr(this.bytesBitwiser.ShiftLeft(k1, 1), this.constRb);
            }
            string result = this.hexConverter.Bytes2Hex(k2);
            Debug.WriteLine("Expect:\t" + expected);
            Debug.WriteLine("Result:\t" + result);
            Assert.AreEqual(expected, result);
        }

        [TestCleanup]
        public void Clear()
        {

        }
    }
}
