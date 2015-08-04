using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.EskmsAPI;
using Crypto.CommonUtility;
using System.Diagnostics;
//
//using Common.Logging;

namespace Crypto.EskmsAPI_UnitTest
{
    [TestClass]
    public class iBonAuthenticate_UnitTest //: TraceListener
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(iBonAuthenticate_UnitTest));
        private IiBonAuthenticate iBonAuth { get; set; }

        private IHexConverter hexConverter { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.hexConverter = new HexConverter();

            this.iBonAuth = new iBonAuthenticate();
            //input data
            this.iBonAuth.Input_KeyLabel = "2ICH3F000032A";
            this.iBonAuth.Input_KeyVersion = "0";
            this.iBonAuth.Input_BlobValue = this.hexConverter.Hex2Bytes("0104873ABA8D2C80494341534804873A9B330A45CCB51DDE66FDD7EABD400895");
            this.iBonAuth.Input_Enc_RanB = "4EF61041ABE8B0EF8B32A627B19D83AA";//加密的RanB
            this.iBonAuth.Input_RanA = new byte[] { 0x7A,0x0F,0x1F,0xBC,0xD5,0xFD,0xEA,0x04,
                                                    0x8C,0x9B,0xD7,0x90,0x05,0x0E,0x78,0xA0 };//固定RanA值

        }

        [TestMethod]
        public void TestMethod_StartAuthenticate()
        {
            string expectedRanBStr = "65ADC0C88F7BFB97430D6F84274FC376";//預期解密的RanB字串
            byte[] expectedRanB = this.hexConverter.Hex2Bytes(expectedRanBStr);//預期的RanB
            string expectedDivKey = "17AB67F130169FB3C012B2DD17985365";//Kx(從KMS取得的DivKey)
            string expectedEnc_RanAandRanBRol8Str = "B6EE87D2F942E2CB70EF6605CBA463EAF605E369EB6036600C2E6F2D528E475B";//預期的Enc(iv, RanA || RanBRol8)
            byte[] expectedEnc_RanAandRanBRol8 = this.hexConverter.Hex2Bytes(expectedEnc_RanAandRanBRol8Str);

            string expectedEncRanARol8Str = "CADB06F55D182E9CB5DF7B8246C991D1";//E(RanARol8) //文件上的加密過的(RanA左旋 1 byte)
            byte[] expectedEncRanARol8 = this.hexConverter.Hex2Bytes(expectedEncRanARol8Str);

            string expectedSessionKeyStr = "7A0F1FBC65ADC0C8050E78A0274FC376";//預期的SessionKey
            byte[] expectedSessionKey = this.hexConverter.Hex2Bytes(expectedSessionKeyStr);
            Trace.WriteLine("預期DivKey:" + expectedDivKey);
            //************************************************************
            //開始執行認證
            this.iBonAuth.StartAuthenticate(true);
            //************************************************************
            //檢查解密後的RanB
            for (int i = 0; i < expectedRanB.Length; i++)
            {
                Assert.AreEqual(expectedRanB[i], this.iBonAuth.Output_RanB[i], 
                    "預期RanB[" + i + "]:" + expectedRanB[i] + " 不同於實際RanB[" + i + "]:" + this.iBonAuth.Output_RanB[i]);
            }

            //檢查E(iv,RanA||RanBRol8); iv = E(RanB)
            for(int j = 0 ; j < expectedEnc_RanAandRanBRol8.Length;j++)
            {
                Assert.AreEqual(expectedEnc_RanAandRanBRol8[j], this.iBonAuth.Output_Enc_RanAandRanBRol8[j],
                    "預期Enc_RanAandRanBRol8[" + j + "]:" + expectedEnc_RanAandRanBRol8[j] + 
                    " 不同於實際Enc_RanAandRanBRol8[" + j + "]:" + this.iBonAuth.Output_Enc_RanAandRanBRol8[j]);
            }

            //檢查E(iv,RanARol8)
            for (int k = 0; k < expectedEncRanARol8.Length; k++)
            {
                Assert.AreEqual(expectedEncRanARol8[k], this.iBonAuth.Output_Enc_IVandRanARol8[k],
                    "預期Enc_IVandRanARol8[" + k + "]:" + expectedEncRanARol8[k] +
                    " 不同於實際Enc_IVandRanARol8[" + k + "]:" + this.iBonAuth.Output_Enc_IVandRanARol8[k]);
            }

            //檢查最終的 Session Key
            for (int m = 0; m < expectedSessionKey.Length; m++)
            {
                Assert.AreEqual(expectedSessionKey[m], this.iBonAuth.Output_SessionKey[m],
                    "預期Enc_IVandRanARol8[" + m + "]:" + expectedSessionKey[m] +
                    " 不同於實際Enc_IVandRanARol8[" + m + "]:" + this.iBonAuth.Output_SessionKey[m]);
            }

            //Debug.WriteLine("Random A Start Index:" + this.iBonAuth.Output_RandAStartIndex);//因為使用固定的RanA
        }

        [TestCleanup]
        public void Clear()
        {

        }

        #region 為了監聽Debug內的WriteLine繼承後實作TraceListener
        //public override void Write(string message)
        //{
        //    log.Debug(message);
        //}

        //public override void WriteLine(string message)
        //{
        //    log.Debug(message);
        //}
        #endregion
    }
}
