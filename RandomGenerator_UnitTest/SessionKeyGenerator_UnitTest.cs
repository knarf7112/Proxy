using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using RandomGenerator;
using System.Diagnostics;

namespace RandomGenerator_UnitTest
{
    [TestClass]
    public class SessionKeyGenerator_UnitTest
    {
        private ISessionKeyGenerator sessionKeyGenerator { get; set; }

        private int RanAStartIndex { get; set; }

        [TestInitialize]
        public void Init()
        {
            this.sessionKeyGenerator = new SessionKeyGenerator();
            this.RanAStartIndex = 11;
        }

        [TestMethod]
        public void TestMethod_GetRanA()
        {
            byte[] expected = new byte[] { 0x8E, 0xEB, 0x6A, 0x5E, 0x1E, 0xE9, 0xB3, 0x14, 
                                           0x71, 0xC2, 0xE9, 0x96, 0xC3, 0x92, 0xF9, 0x43 };

            byte[] specifiedBytes = sessionKeyGenerator.GetRanA(RanAStartIndex);//取得指定起始位置的連續Block大小陣列(16 bytes)

            Debug.WriteLine("預期RanA[" + this.RanAStartIndex + "]:\t" + BitConverter.ToString(expected).Replace("-", ""));
            Debug.WriteLine("實際RanA[" + this.RanAStartIndex + "]:\t" + BitConverter.ToString(specifiedBytes).Replace("-", ""));
            for (int i = 0; i < expected.Length; i++)
            {
                    Assert.AreEqual(expected[i],specifiedBytes[i],"錯誤索引(" + i + ")預期陣列值:" + expected[i] + " != 指定陣列值:" + specifiedBytes[i]);
            }
            Debug.WriteLine("Finished compare bytes!");

            //rng.WriteFile(4096);//產生隨機檔案,用來複製的
            //Console.ReadKey();
        }

        [TestMethod]
        public void TestMethod_ListAllRanA()
        {
            int sessionKeyDataLength = 16;
            string digit = this.sessionKeyGenerator.GetTotalLength().ToString().Length.ToString();//get length digit;取得陣列長度的個數
            for (int i = 0; i < this.sessionKeyGenerator.GetTotalLength() - sessionKeyDataLength; i++)
            {
                byte[] sessionKey = this.sessionKeyGenerator.GetRanA(i);
                Debug.WriteLine("RanA[" + i.ToString(("D" + digit)) + "]:\t" + BitConverter.ToString(sessionKey).Replace("-", ""));//列表所有RandA
            }
        }

        [TestMethod]
        public void TestMethod_GetSessionKey()
        {
            byte[] expectedRanA = new byte[] { 
                0x8E, 0xEB, 0x6A, 0x5E, 0x1E, 0xE9, 0xB3, 0x14, 
                0x71, 0xC2, 0xE9, 0x96, 0xC3, 0x92, 0xF9, 0x43 
            };
            int expectedRanAStartIndex = 11;
            byte[] expectedRanB = new byte[]{
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            };
            //SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
            byte[] expectedSessionKey = new byte[]{
                0x8E, 0xEB, 0x6A, 0x5E, 0x00, 0x01, 0x02, 0x03,
                0xC3, 0x92, 0xF9, 0x43, 0x0C, 0x0D, 0x0E, 0x0F
            };
            byte[] actualSessionKey = sessionKeyGenerator.GetSessionKey(this.RanAStartIndex, expectedRanB);
            Debug.WriteLine("預期SessionKey:\t" + BitConverter.ToString(expectedSessionKey).Replace("-", ""));
            Debug.WriteLine("實際SessionKey:\t" + BitConverter.ToString(actualSessionKey).Replace("-", ""));
            for (int i = 0; i < expectedSessionKey.Length; i++)
            {
                Assert.AreEqual(expectedSessionKey[i],actualSessionKey[i], "錯誤索引(" + i + ")預期陣列值:" + expectedSessionKey[i] + " != 指定陣列值:" + actualSessionKey[i]);
            }
            Assert.AreEqual(expectedRanAStartIndex, this.RanAStartIndex, "預期RanA起始索引值[" + expectedRanAStartIndex + "]不同於實際起始索引值[" + this.RanAStartIndex + "]");
        }

        [TestCleanup]
        public void Clear()
        {

        }
    }
}
