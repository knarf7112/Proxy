using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//SessionKey Handler
using Crypto.ZMK.Session;
using System.Diagnostics;
using RandomGenerator;
namespace Crypto.ZMK.Session_UnitTest
{
    [TestClass]
    public class UnitTest_KeyManager
    {
        private IKeyManager keyManager;
        private IRndGenerator rndObj;
        [TestInitialize]
        public void Init()
        {
            this.keyManager = new SessionKeyManager();
            this.rndObj = new RndGenerator();
        }
        [TestMethod]
        public void TestMethod_Gen_AB_Part_and_Get_SessionKey()
        {
            //SessionKey轉成A_part索引和B_part陣列再轉回SessionKey,比較SessionKey是否相同
            byte[] guid = Guid.NewGuid().ToByteArray();
            int a_part = -1;
            byte[] b_part = null;
            byte[] ab_to_SessionKey = null;
            this.keyManager.Gen_AB_Part(guid, out a_part, out b_part);
            Debug.WriteLine(String.Format("SessionKey:{0} ,A_part:{1} ,B_Part:{2}",
                BitConverter.ToString(guid).Replace("-",""),
                a_part.ToString(),
                BitConverter.ToString(b_part).Replace("-","")));
            if (this.keyManager.Get_SessionKey(a_part, b_part,out ab_to_SessionKey))
            {
                Debug.WriteLine(String.Format("原始的SessionKey:{0}\r\n還原的SessionKey:{1}",
                BitConverter.ToString(guid).Replace("-", ""),
                BitConverter.ToString(ab_to_SessionKey).Replace("-", "")));
                Assert.IsTrue(Comparer(guid, ab_to_SessionKey));
            }
            else
            {
                Assert.Fail("A B 轉回SessionKey失敗");
            }
        }

        [TestMethod]
        public void TestMethod_SessionKey_Encrypt_data_and_Decrypt_data()
        {
            //測試ZMK_data轉SessionKey後的索引値是否存在隨機物件內
            byte[] zmk_data = Guid.NewGuid().ToByteArray();
            byte[] sessionKey = null;
            byte[] expected_random = null;
            byte[] actual_random = null;
            int rndom_index = -1;//設定隨機物件內的索引値

            //zmk(key) + 0(IV) + RnA(data) => sessionKey
            this.keyManager.Encrypt_data(zmk_data, out rndom_index, out sessionKey);
            expected_random = this.rndObj.Get_RandomFromIndex(rndom_index);
            Debug.WriteLine(String.Format("ZMK_data:{0}\r\n隨機物件內的索引値:{1}\r\nSessionKey:{2}",
                BitConverter.ToString(zmk_data).Replace("-", ""),
                rndom_index.ToString(),
                BitConverter.ToString(sessionKey).Replace("-", "")));
            //zmk(key) + 0(IV) + sessionKey(data) => RnA
            actual_random = this.keyManager.Decrypt_data(zmk_data, sessionKey);


            Debug.WriteLine(String.Format("原始的Random:{0}\r\n還原的Random:{1}",
                BitConverter.ToString(expected_random).Replace("-", ""),
                BitConverter.ToString(actual_random).Replace("-", "")));
            Assert.IsTrue(Comparer(expected_random, actual_random));
        }

        [TestMethod]
        public void TestMethod_DiverseKey_Encrypt_data_and_Decrypt_data()
        {
            //測試使用sessionKey加密DiverseKey後,再將加密的DiverseKey轉回
            byte[] sessionKey = Guid.NewGuid().ToByteArray();
            byte[] expected_diverseKey = Guid.NewGuid().ToByteArray();
            byte[] encrypted_DiverseKey = null;
            byte[] actual_diverseKey = null;
            int iv_rndom_index = -1;//設定隨機物件內的索引値

            //sessionKey(key) + RnB(IV) + diverseKey(data) => encrypted_DiverseKey
            this.keyManager.Encrypt_data(sessionKey, out iv_rndom_index, out encrypted_DiverseKey, expected_diverseKey);
            Debug.WriteLine(String.Format("sessionKey:{0}\r\n隨機物件內的索引値:{1}\r\nDiverseKey:{2}\r\n加密的DiverseKey:{3}",
                BitConverter.ToString(sessionKey).Replace("-", ""),
                iv_rndom_index.ToString(),
                BitConverter.ToString(expected_diverseKey).Replace("-", ""),
                BitConverter.ToString(encrypted_DiverseKey).Replace("-", "")));

            //sessionKey(key) + RnB(IV) + encrypted_DiverseKeydiverseKey(data) => diverseKey
            actual_diverseKey = this.keyManager.Decrypt_data(sessionKey, encrypted_DiverseKey,iv_rndom_index);

            Debug.WriteLine(String.Format("原始的DiverseKey:{0}\r\n還原的DiverseKey:{1}",
                BitConverter.ToString(expected_diverseKey).Replace("-", ""),
                BitConverter.ToString(actual_diverseKey).Replace("-", "")));
            Assert.IsTrue(Comparer(expected_diverseKey, actual_diverseKey));
        }

        public bool Comparer(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }

        [TestCleanup]
        public void Clear()
        {

        }
    }
}
