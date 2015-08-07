using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crypto.EskmsAPI;
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto.EskmsAPI_UnitTest
{
    [TestClass]
    public class AesCMac2Worker_UnitTest
    {
        private AesCMac2Worker _AesCMac2Worker;

        private HexConverter hexConverter;

        [TestInitialize]
        public void Init()
        {
            this.hexConverter = new HexConverter();
            this._AesCMac2Worker = new AesCMac2Worker();
        }

        [TestMethod]
        public void TestMethod_()
        {
            string expected = "17AB67F130169FB3C012B2DD17985365";
            this._AesCMac2Worker.SetIv(AesCMac2Worker.ConstZero);
            this._AesCMac2Worker.SetMacKey("2ICH3F000032A");//0032=>0xA的Key查(二代I-Cash卡發卡系統規格書V1.10@20130107.pdf)的page
            this._AesCMac2Worker.DataInput(this.hexConverter.Hex2Bytes("0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80"));
            byte[] result = this._AesCMac2Worker.GetMac();
            Assert.AreEqual(expected, this.hexConverter.Bytes2Hex(result));
            Debug.WriteLine("Result:[" + this.hexConverter.Bytes2Hex(result) + "]");
        }

        [TestCleanup]
        public void Clear()
        {

        }
    }
}
