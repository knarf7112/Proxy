using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//ZMK Manager
using Crypto.ZMK;
//Debug Object
using System.Diagnostics;

namespace Crypto.ZMK_UnitTest
{
    [TestClass]
    public class UnitTest_Manager
    {
        IZMK_Manager manager;

        [TestInitialize]
        public void Init()
        {
            string keyLabel = "2ICH3F000002A";
            byte[] iv = null;//EsKmsWebApi使用ECB不用帶IV//new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            this.manager = new ZMK_Manager(keyLabel, iv);
        }
        [TestMethod]
        public void TestMethod_ZMKEncrypto()
        {
            int count = 100;// Connect KMS 2.0 Count;
            string encZmk_data = String.Empty;
            HashSet<string> encZMK_DataList = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                byte[] encZMK_data = this.manager.GetEncrypt_ZMK_Data();
                encZmk_data = BitConverter.ToString(encZMK_data).Replace("-", "");
                Debug.WriteLine(String.Format("Encrypted ZMK_data:{0}", encZmk_data));
                encZMK_DataList.Add(encZmk_data);
            }
            //check ZMK data are Unique
            Assert.AreEqual(count, encZMK_DataList.Count);
        }

        [TestMethod]
        public void TestMethod_ZMKDecrypto()
        {
            int count = 100;// Connect KMS 2.0 Count;
            string encZmk_data = String.Empty;
            for (int i = 0; i < count; i++)
            {
                byte[] encZMK_data = this.manager.GetEncrypt_ZMK_Data();
                byte[] decrypted_ZMK_data = this.manager.GetDecrypt_ZMK_Data(encZMK_data);
                Debug.WriteLine(String.Format("第{0}筆 Encrypted ZMK_data:{1}", i, BitConverter.ToString(encZMK_data).Replace("-", "")));
                Debug.WriteLine(String.Format("第{0}筆 Decrypted ZMK_data:{1}", i, BitConverter.ToString(decrypted_ZMK_data).Replace("-", "")));
                //要去把_ZMK_Data注解(PS:不能顯示的重要資料)取消,比較取得ZMK_DATA紀錄的欄位値是否同KMS解密後的値
                Assert.IsTrue(Comparer(((ZMK_Manager)this.manager)._ZMK_Data, decrypted_ZMK_data));
            }
        }

        [TestMethod]
        public void TestMethod_Get_XOR_data()
        {
            byte[] expected_data1 = new byte[] { 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01 };
            byte[] expected_data2 = this.manager.GetEncrypt_ZMK_Data();
            byte[] xor_data = this.manager.Get_XOR_data(expected_data1, expected_data2);
            Debug.WriteLine("data1:\t\t{0}\r\ndata2:\t\t{1}\t\r\nXor data:\t{2}", BitConverter.ToString(expected_data1).Replace("-", ""), BitConverter.ToString(expected_data2).Replace("-", ""), BitConverter.ToString(xor_data).Replace("-", ""));
            //check back up
            byte[] actual_data1 = this.manager.Get_XOR_data(xor_data, expected_data2);
            byte[] actual_data2 = this.manager.Get_XOR_data(xor_data, expected_data1);
            byte[] actual_xor_data = this.manager.Get_XOR_data(actual_data1, actual_data2);
            Assert.IsTrue(Comparer(expected_data1, actual_data1));
            Assert.IsTrue(Comparer(expected_data2, actual_data2));
            Assert.IsTrue(Comparer(xor_data, actual_xor_data));
        }

        [TestMethod]
        public void TestMethod_Get_AB_Part()
        {
            byte[] a_part = null;
            byte[] b_part = null;
            byte[] zmk_data = null;
            byte[] ab_to_zmk = null;
            bool success = ((ZMK_Manager)this.manager).Get_AB_Part(zmk_data, out a_part, out b_part);
            Assert.IsFalse(success, "zmk_data 沒輸入應無法產出A B part");
            Assert.IsNull(a_part);
            Assert.IsNull(b_part);

            //ZMK DATA : 16 bytes 建一個隨機ZMK DATA
            zmk_data = ((ZMK_Manager)this.manager).Gen_Rnd16();
            success = ((ZMK_Manager)this.manager).Get_AB_Part(zmk_data, out a_part, out b_part);
            Assert.IsTrue(success, "zmk_data 產生A B part時發生異常");
            Assert.IsNotNull(a_part);
            Assert.IsNotNull(b_part);
            ab_to_zmk = this.manager.Get_XOR_data(a_part, b_part);//A B碼單反轉回來ZMK
            Assert.IsTrue(Comparer(zmk_data, ab_to_zmk));//要一樣
            Debug.WriteLine(String.Format("隨機一組ZMK_DATA:\t{0}", BitConverter.ToString(zmk_data).Replace("-", "")));
            Debug.WriteLine(String.Format("A Part DATA:\t\t{0}", BitConverter.ToString(a_part).Replace("-", "")));
            Debug.WriteLine(String.Format("B Part DATA:\t\t{0}", BitConverter.ToString(b_part).Replace("-", "")));
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
