using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.EskmsAPI;
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto.EskmsAPI_UnitTest
{
	[TestClass]
	public class KMSGetter_UnitTest
	{

        //private static readonly ILog log = LogManager.GetLogger(typeof(iBonAuthenticate_UnitTest));
        private IKMSGetter kmsGetter { get; set; }

        private IHexConverter hexConverter { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.hexConverter = new HexConverter();

            this.kmsGetter = new KMSGetter();
            //input data
            this.kmsGetter.Input_KeyLabel = "2ICH3F000002A";// "2ICH3F000032A";
            this.kmsGetter.Input_KeyVersion = "0";
            this.kmsGetter.Input_UID = "043137328D3780";// "043132328D3780";
            //使用未導出k0的uid來作,所以是01+uid+ICASH+uid+ICASH+uid => "0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80"
            //this.kmsGetter.Input_BlobValue = this.hexConverter.Hex2Bytes("01043132328D37804943415348043132328D37804943415348043132328D3780");// ("0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80");/*"0104214C82583B80494341534804214C82583B80494341534804214C82583B80");*///"0104873ABA8D2C80494341534804873A9B330A45CCB51DDE66FDD7EABD400895");//

        }
        [TestMethod]
		public void TestMethod1()
        {
            string expectedDivKey = "0254410BFC9402D8CBAF9EBC7BBB4E4C";// "1FCDE1004E4AB3268EF690F543B94429";//"0D8B0DAFDA0C733839B14A377E51559E";//Kx(從KMS取得的DivKey)
            Debug.WriteLine("預期DivKey:" + expectedDivKey);//EE0E90B75F83D7F2C52D3C5F4CAF20D3
            //開始執行認證
            byte[] diversKey = this.kmsGetter.GetDiversKey();
            string actualDivKey = BitConverter.ToString(diversKey).Replace("-","");
            Assert.AreEqual(expectedDivKey, actualDivKey);
            /*  bytSecretKey
             * [0x00000000]: 0x1f
               [0x00000001]: 0xcd
               [0x00000002]: 0xe1
               [0x00000003]: 0x00
               [0x00000004]: 0x4e
               [0x00000005]: 0x4a
               [0x00000006]: 0xb3
               [0x00000007]: 0x26
               [0x00000008]: 0x8e
               [0x00000009]: 0xf6
               [0x0000000a]: 0x90
               [0x0000000b]: 0xf5
               [0x0000000c]: 0x43
               [0x0000000d]: 0xb9
               [0x0000000e]: 0x44
               [0x0000000f]: 0x29
           */

		}
	}
}
