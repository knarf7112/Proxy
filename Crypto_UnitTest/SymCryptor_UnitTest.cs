using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto;
using Crypto.CommonUtility;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
namespace Crypto_UnitTest
{
    [TestClass]
    public class SymCryptor_UnitTest
    {
        private IHexConverter hexConverter;
        private ISymCryptor symCryptor;
        private IPaddingHelper paddingHelper;
        private IByteWorker byteWorker;

        [TestInitialize]
        public void Init()
        {
            this.hexConverter = new HexConverter();
            this.symCryptor = new SymCryptor("AES");
            this.paddingHelper = new Pkcs7PaddingHelper();
            this.byteWorker = new ByteWorker();
            // aes-128-cbc
            //byte[] key = {145,12,32,245,98,132,98,214,6,77,131,44,221,3,9,50};
            //KEY(128|192|256,128bit):910C20F5628462D6064D832CDD030932
            // AES-192-CBC
            //***************************************
            string keyString = "0123456789abcdef";//AES用的加解密金鑰字串
            string hexKeyString = this.hexConverter.Str2Hex(keyString);//AES用的加解密金鑰轉hex
            Debug.WriteLine("Hex Key String:" + hexKeyString);
            //***************************************
            byte[] key = this.hexConverter.Hex2Bytes //AES用的加解密金鑰轉Byte Array(固定長度: 16 bytes)
            (
                // "00112233445566778899AABBCCDDEEFF0102030405060708"
               //"3FE9489CC954E7DA1B4806A8133B81EB"
               hexKeyString//16 bytes
            );
            //
            //byte[] iv = {15,122,132,5,93,198,44,31,9,39,241,49,250,188,80,7};
            //IV(128bit):0F7A84055DC62C1F0927F131FABC5007
            //IV: ALL ZERO
            //byte[] iv = new byte[16];
            //for (int i = 0; i < iv.Length; i++)
            //{
            //    iv[i] = 0;
            //}
            //
            this.symCryptor.SetIV(SymCryptor.ConstZero);
            this.symCryptor.SetKey(key);
            //log.Debug( "KEY:" + this.hexConverter.Bytes2Hex( key ) );
            //log.Debug( "IV:" + this.hexConverter.Bytes2Hex( iv ) );
        }

        [TestMethod]
        public void Test_Encrypt2Base64()
        {
            string dataStr = "test1234567890QQ";
            string hexData = this.hexConverter.Str2Hex(dataStr);
            byte[] data = this.hexConverter.Hex2Bytes(hexData);
            //*************************************
            //byte[] data = { 0x1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            //byte[] data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//16個
            byte[] srcBytes = data;
            //若資料尾部有剩餘
            if ((data.Length % 16) != 0)
            {
                srcBytes = new byte[((data.Length / 16) + 1) * 16];//補一個Block
                Array.Copy(data, srcBytes, data.Length);
                for (int i = data.Length; i < srcBytes.Length; i++)
                {
                    srcBytes[i] = 0x20;//補空白 0x20=空白 
                }
            }

            //string expected = "10yVz/WfSb9KfoWeCisNyQ==";
            string expected = "6Wo1OsWByVnZ823T4/Y4Lg==";//"Utta/ntk7/qx6S7qmDxfcw==";
            //*********************************************
            byte[] encData = this.symCryptor.Encrypt(srcBytes);
            string encHex = this.hexConverter.Bytes2Hex(encData);
            byte[] decData = this.symCryptor.Decrypt(encData);
            string decHex = this.hexConverter.Bytes2Hex(decData);
            //*********************************************
            string base64Result = Convert.ToBase64String(this.symCryptor.Encrypt(srcBytes));
            Debug.WriteLine("Base64 String: " + base64Result);
            Debug.WriteLine(this.hexConverter.Bytes2Hex(Convert.FromBase64String(base64Result)));

            Assert.AreEqual(expected, base64Result);
            //
            byte[] result = this.symCryptor.Decrypt(Convert.FromBase64String(base64Result));
            for (int srcIndex = 0; srcIndex < srcBytes.Length; srcIndex++)
            {
                Assert.AreEqual(srcBytes[srcIndex], result[srcIndex]);
            }
        }

        [TestMethod]
        public void Test_Encrypt2Hex()
        {
            string dataStr = "test1234567890QQ";
            Debug.WriteLine("原始資料:\t" + dataStr);
            string hexData = this.hexConverter.Str2Hex(dataStr);
            byte[] data = this.hexConverter.Hex2Bytes(hexData);
            //all zero
            //byte[] data = new byte[16];
            //for (int i = 0; i < data.Length; i++)
            //{
            //    data[i] = 0;
            //}
            string expected = "E96A353AC581C959D9F36DD3E3F6382E";
            //
            
            byte[] resultBytes = this.symCryptor.Encrypt(data);//data加密
            string hexResult = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine("加密資料:\t" + hexResult);
            //*********************************************************************
            byte[] decBytes = this.symCryptor.Decrypt(resultBytes);//加密資料解密看看
            string decStr = Encoding.ASCII.GetString(decBytes);
            Debug.WriteLine("解密資料:\t" + decStr);
            Assert.AreEqual(dataStr, decStr);//解密後的字串是否和原始字串相同
            //*********************************************************************
            Assert.AreEqual(expected, hexResult);
        }

        private byte[] getSessionKey()
        {
            // rndA fixed 
            byte[] rndA = this.hexConverter.Hex2Bytes("1234567890ABCDEF1234567890ABCDEF");
            // rndB will change each time
            byte[] rndB = new byte[]
            {
                0xE6,0xEA,0x14,0x3C
               ,0xCB,0x47,0xEA,0xB5
               ,0x69,0xC6,0x0B,0xE2
               ,0x6A,0xAE,0x12,0xD5
            }
            ;
            //return this.hexConverter.Hex2Bytes( "00000000000000000000000000000031" );
            return this.byteWorker.Combine
            (
                 this.byteWorker.SubArray(rndA, 0, 4)
               , this.byteWorker.SubArray(rndB, 4, 4)
               , this.byteWorker.SubArray(rndA, 8, 4)
               , this.byteWorker.SubArray(rndB, 12, 4)
            );
        }

        [TestMethod]
        public void Test_PaddingEncrypt()
        {
            byte[] sesKey = this.getSessionKey();
            Debug.WriteLine("ses key:" + this.hexConverter.Bytes2Hex(sesKey));
            this.symCryptor.SetKey(sesKey);
            this.symCryptor.SetIV(SymCryptor.ConstZero);
            Debug.WriteLine("IV:" + this.hexConverter.Bytes2Hex(SymCryptor.ConstZero));
            //
            string decryptedHex =
                "3838303130303031" + "81A400000330000200";

            byte[] data = this.hexConverter.Hex2Bytes(decryptedHex);
            string expected =
                "2090E1E2DFA15DD9F74C90B3A17E6A646617DB7F8BF5845E1E4909051DD6232A";

            byte[] resultBytes = this.symCryptor.Encrypt(this.paddingHelper.AddPadding(data));
            string hexResult = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine(hexResult);
            Assert.AreEqual(expected, hexResult);
        }

        [TestMethod]
        public void Test_PaddingDecrypt()
        {
            byte[] sesKey = this.getSessionKey(); //"12345678F1E2D3C412345678F1E2D3C4";
            this.symCryptor.SetKey(sesKey);
            this.symCryptor.SetIV(SymCryptor.ConstZero);
            Debug.WriteLine("ses key:" + this.hexConverter.Bytes2Hex(sesKey));
            Debug.WriteLine("IV:" + this.hexConverter.Bytes2Hex(SymCryptor.ConstZero));
            //
            string encryptedHex =
                // "2090E1E2DFA15DD9F74C90B3A17E6A646617DB7F8BF5845E1E4909051DD6232A";
               "3855536C2FB9D7526BD5F5DD7A74CDC22DD537B453073BC39307BCE543A86E5BA370BB2F4A2DC77C1885B5DE3413A8C46E60021FB8FACBABDA8B0B6D917A7207A07F20641D22C2A75D1077E4FF9D9422BA33CDE6CAB63A5E74F9BBF703762CFB55B7B7772B6B31C022D7BEBC62A9E430769C7B9E7B803A987D44A05119F605B95CC0F7B253C81CB7A7751C6628B46289";

            byte[] data = this.hexConverter.Hex2Bytes(encryptedHex);
            byte[] resultBytes0 = this.symCryptor.Decrypt(data);
            Debug.WriteLine("orig bytes:" + this.hexConverter.Bytes2Hex(resultBytes0));
            byte[] resultBytes = this.paddingHelper.RemovePadding(resultBytes0);
            string hexResult = this.hexConverter.Bytes2Hex(resultBytes);
            Debug.WriteLine("unpadding :" + hexResult);
            string expected = "3838303130303031" + "81A400000330000200";
            Assert.AreEqual(expected, hexResult);
        }

        [TestMethod]
        public void Test01_EncryptFile()
        {
            //byte[] sesKey = this.getSessionKey();
            //log.Debug("ses key:" + this.hexConverter.Bytes2Hex(sesKey));
            //this.symCryptor.SetKey(sesKey);
            //this.symCryptor.SetIv(SymCryptor.ConstZero);
            //log.Debug("IV:" + this.hexConverter.Bytes2Hex(SymCryptor.ConstZero));
            //
            Assembly assembly = Assembly.Load("Kms.Crypto.UnitTest");
            using (Stream src = assembly.GetManifestResourceStream(@"Kms.Crypto.UnitTest.dec_BRQA_567_20141225_B06B_01"))
            {
                using (Stream padSrc = new FileStream(@"d:\temp\pad.dat", FileMode.Create, FileAccess.Write))
                {
                    this.paddingHelper.AddPadding(src, padSrc);
                }
            }
            using (Stream padSrc = new FileStream(@"d:\temp\pad.dat", FileMode.Open, FileAccess.Read))
            {
                using (Stream enc = new FileStream(@"d:\temp\enc.dat", FileMode.Create, FileAccess.Write))
                {
                    this.symCryptor.Encrypt(padSrc, enc);
                }
            }
            File.Delete(@"d:\temp\pad.dat");
        }

        [TestMethod]
        public void Test02_DecryptFile()
        {
            string tid = "860406320AAB3680";
            this.symCryptor.SetIV(SymCryptor.ConstZero);
            this.symCryptor.SetKey(this.hexConverter.Hex2Bytes(tid + tid));
            //
            using (Stream encSrc = new FileStream(@"d:\temp\860406320AAB3680.enc", FileMode.Open, FileAccess.Read))
            {
                using (Stream padDecSrc = new FileStream(@"d:\temp\pad.dat", FileMode.Create, FileAccess.Write))
                {
                    this.symCryptor.Decrypt(encSrc, padDecSrc);
                }
            }
            using (Stream padDecSrc = new FileStream(@"d:\temp\pad.dat", FileMode.Open, FileAccess.Read))
            {
                using (Stream decSrc = new FileStream(@"d:\temp\nopad.dat", FileMode.Create, FileAccess.Write))
                {
                    this.paddingHelper.RemovePadding(padDecSrc, decSrc);
                }
            }
            //File.Delete(@"d:\temp\pad.dat");
        }

        [TestCleanup]
        public void CLear()
        {

        }
    }
}
