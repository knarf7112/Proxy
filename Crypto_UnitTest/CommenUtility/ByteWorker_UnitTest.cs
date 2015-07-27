using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto_UnitTest.CommenUtility
{
    [TestClass]
    public class ByteWorker_UnitTest
    {
        private IHexConverter hexConverter;
        private IByteWorker byteWorker;

        [TestInitialize]
        public void Init()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
        }

        [TestMethod]
        public void Test_SubArray()
        {
            //24byte key: RndA(0-7)+RndB(0-3)+RndA(8-11)+RndB(8-15)
            string ra = "1234567890ABCDEF1234567890ABCDEF";
            string rb = "F1E2D3C4B5A60798F1E2D3C4B5A60798";
            string expected = "1234567890ABCDEF";
            byte[] randomA = this.hexConverter.Hex2Bytes(ra);//資料轉回資料陣列
            byte[] rndA0_7 = this.byteWorker.SubArray(randomA, 0, 8);//取資料陣列的0~7
            Debug.WriteLine("Expect:\t" + expected);
            Assert.AreEqual(expected, this.hexConverter.Bytes2Hex(rndA0_7));//結果是否一致
            //
            byte[] randomB = this.hexConverter.Hex2Bytes(rb);
            byte[] rndB0_3 = this.byteWorker.SubArray(randomB, 0, 4);
            expected = "F1E2D3C4";
            Debug.WriteLine("Expect:\t" + expected);
            Assert.AreEqual(expected, this.hexConverter.Bytes2Hex(rndB0_3));
            //
            byte[] rndA8_11 = this.byteWorker.SubArray(randomA, 8, 4);
            expected = "12345678";
            Debug.WriteLine("Expect:\t" + expected);
            Assert.AreEqual(expected, this.hexConverter.Bytes2Hex(rndA8_11));
            //
            byte[] rndB8_15 = this.byteWorker.SubArray(randomB, 8, 8);
            expected = "F1E2D3C4B5A60798";
            Debug.WriteLine("Expect:\t" + expected);
            Assert.AreEqual(expected, this.hexConverter.Bytes2Hex(rndB8_15));
        }

        [TestMethod]
        public void Test_RotateLeft()
        {
            string orig = "1234567890ABCDEF1234567890ABCDEF";
            string expected = "567890ABCDEF1234567890ABCDEF1234";

            byte[] origBytes = this.hexConverter.Hex2Bytes(orig);
            // rotate byte left 2 times  
            byte[] resultBytes = this.byteWorker.RotateLeft(origBytes, 2);

            string result = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine("orig: \t" + orig);
            Debug.WriteLine("Expect:\t" + expected);
            Debug.WriteLine("Result:\t" + result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_RotateRight()
        {
            string orig = "D3C4B5A60798F1E2D3C4B5A60798F1E2";
            string expected = "F1E2D3C4B5A60798F1E2D3C4B5A60798";

            byte[] origBytes = this.hexConverter.Hex2Bytes(orig);
            // rotate byte right 2 times  
            byte[] resultBytes = this.byteWorker.RotateRight(origBytes, 2);

            string result = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine("orig: \t" + orig);
            Debug.WriteLine("Expect:\t" + expected);
            Debug.WriteLine("Result:\t" + result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_CombineTwoArray()
        {
            string ra = "1234567890ABCDEF1234567890ABCDEF";
            string rb = "F1E2D3C4B5A60798F1E2D3C4B5A60798";
            string expected = "F1E2D3C4B5A60798F1E2D3C4B5A60798567890ABCDEF1234567890ABCDEF1234";

            // Combine Rb + Ra' 
            byte[] resultBytes = this.byteWorker.Combine
            (
                this.hexConverter.Hex2Bytes(rb),
                this.byteWorker.RotateLeft(this.hexConverter.Hex2Bytes(ra), 2)
            );

            string result = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine("Expect:\t" + expected);
            Debug.WriteLine("Result:\t" + result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test_CombineMultiArray()
        {
            //24byte key: RndA(0-7)+RndB(0-3)+RndA(8-11)+RndB(8-15)
            string ra = "1234567890ABCDEF1234567890ABCDEF";
            string rb = "F1E2D3C4B5A60798F1E2D3C4B5A60798";
            string expected = "1234567890ABCDEFF1E2D3C412345678F1E2D3C4B5A60798";
            //
            byte[] randomA = this.hexConverter.Hex2Bytes(ra);
            byte[] randomB = this.hexConverter.Hex2Bytes(rb);
            byte[] resultBytes = this.byteWorker.Combine
            (
                this.byteWorker.SubArray(randomA, 0, 8),
                this.byteWorker.SubArray(randomB, 0, 4),
                this.byteWorker.SubArray(randomA, 8, 4),
                this.byteWorker.SubArray(randomB, 8, 8)
            );
            string result = this.hexConverter.Bytes2Hex(resultBytes);
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
