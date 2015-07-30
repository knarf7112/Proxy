using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.EskmsAPI;
using Crypto.CommonUtility;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Crypto.EskmsAPI_UnitTest
{
    [TestClass]
    public class EsKmsWebApi_UnitTest
    {
        private IEsKmsWebApi esKmsWebApi;

        private IHexConverter hexConverter;

        private IByteWorker byteWorker;
        [TestInitialize]
        public void Init()
        {
            this.esKmsWebApi = new EsKmsWebApi()
            {
                Url = "http://127.0.0.1:8081/eGATEsKMS/interface",//"http://10.27.68.163:8080/eGATEsKMS/interface",
                AppCode = "APP_001",
                AuthCode = "12345678",
                AppName = "icash2Test",
                HttpMethod = "POST",
                HexConverter = new HexConverter(),
                HashWorker = new HashWorker()
                {
                    HashAlg = "SHA1",
                    HexConverter = new HexConverter()
                },
            };

            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
        }

        [TestMethod]
        public void Test_Authenticate()
        {
            //*****************
            // 1.取得DivKey(Kx)
            string expected = "17AB67F130169FB3C012B2DD17985365";//文件上的值,用來比對
            string uid = "04873ABA8D2C80";
            byte[] iv = this.hexConverter.Hex2Bytes("00000000000000000000000000000000");//iv
            byte[] result = this.esKmsWebApi.Encrypt("2ICH3F000032A", iv, this.hexConverter.Hex2Bytes("0104873ABA8D2C80494341534804873A9B330A45CCB51DDE66FDD7EABD400895"));//0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80"));

            Debug.WriteLine("KMS回來的Key:\t\t" + BitConverter.ToString(result));
            byte[] kx = this.byteWorker.SubArray(result, 16, 16);// 取後面 16 byte即DivKey(Kx)
            string mac = BitConverter.ToString(kx);
            Debug.WriteLine("DivKey:\t\t\t" + mac);
            Assert.AreEqual(expected, mac.Replace("-",""));
            

            //*****************
            // 2. 解密RnB
            string encRanB = "4EF61041ABE8B0EF8B32A627B19D83AA";//加密的RanB
            string expectedRanB = "65ADC0C88F7BFB97430D6F84274FC376";//文件上解密的RanB
            SymCryptor symCryptor = new SymCryptor("AES");
            symCryptor.SetIV(SymCryptor.ConstZero);
            symCryptor.SetKey(kx);
            //rndB=D(Kx,iv,encRndB)
            byte[] actualRanB = symCryptor.Decrypt(this.hexConverter.Hex2Bytes(encRanB));
            string ranB = BitConverter.ToString(actualRanB);
            Debug.WriteLine("算出來的RanB:\t\t" + ranB);
            Assert.AreEqual(expectedRanB, ranB.Replace("-", ""));
            //*****************
            // 3. 產生RanA + RanB(左旋 1 Byte)的加密
            //byte[] ranA = new byte[16];
            byte[] ranA = new byte[] { 0x7A,0x0F,0x1F,0xBC,0xD5,0xFD,0xEA,0x04,
                                       0x8C,0x9B,0xD7,0x90,0x05,0x0E,0x78,0xA0 };//文件上的RanA
            //RandomNumberGenerator rnd = new RNGCryptoServiceProvider();
            //rnd.GetBytes(ranA);
            Debug.WriteLine("RanA:\t\t\t" + BitConverter.ToString(ranA));
            byte[] ranBRol8 = this.byteWorker.RotateLeft(actualRanB, 1);//RanB左旋 1 byte
            Debug.WriteLine("RanB左旋8:\t\t" + BitConverter.ToString(ranBRol8));
            //rndARndBROL8 = rndA || rndBROL8
            byte[] ranA_Brol8 = this.byteWorker.Combine(ranA, ranBRol8);
            Debug.WriteLine("RanA + RanB左旋8:\t" + BitConverter.ToString(ranA_Brol8));
            //encRndARndBROL8(32) = E(Kx,iv,rndARndBROL8)
            symCryptor.SetIV(this.hexConverter.Hex2Bytes(encRanB));//
            byte[] enc_ranA_Brol8 = symCryptor.Encrypt(ranA_Brol8);//加密(RanA + RanB左旋8)
            Debug.WriteLine("加密(RanA + RanB左旋8):\t" + BitConverter.ToString(enc_ranA_Brol8));
            //*****************
            // 4. 使用上面尾部16bytes資料當IV後,將RanA左旋8後加密(這要回傳給卡機)
            string expectedEncRanARol8 = "CADB06F55D182E9CB5DF7B8246C991D1";//文件上的加密過的(RanA左旋 1 byte)
            string expectedRanARol8 = "0F1FBCD5FDEA048C9BD790050E78A07A";//文件上的RanA左旋 1 byte
            //iv = last 16 bytes of CApduData
            iv = this.byteWorker.SubArray(enc_ranA_Brol8, 16, 16);
            Debug.WriteLine("改變的IV:\t\t" + BitConverter.ToString(iv));
            symCryptor.SetIV(iv);
            //rndAROL8 = D(Kx,iv,encRndAROL8)
            byte[] ranARol8 = this.byteWorker.RotateLeft(ranA, 1);
            Debug.WriteLine("RanA左旋8:\t\t" + BitConverter.ToString(ranARol8));
            Assert.AreEqual(expectedRanARol8, BitConverter.ToString(ranARol8).Replace("-", ""));
            //encRndAROL8
            byte[] enc_ranARol8 = symCryptor.Encrypt(ranARol8);
            Debug.WriteLine("加密(RanA左旋8):\t\t" + BitConverter.ToString(enc_ranARol8));
            Assert.AreEqual(expectedEncRanARol8, BitConverter.ToString(enc_ranARol8).Replace("-", ""));
            //*****************
            // 5. 輸出傳輸加解密用的Session Key
            //SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
            string expectedSessionKey = "7A0F1FBC65ADC0C8050E78A0274FC376";//文件上的SessionKey
            byte[] A = this.byteWorker.Combine(this.byteWorker.SubArray(ranA, 0, 4), this.byteWorker.SubArray(actualRanB, 0, 4));
            byte[] B = this.byteWorker.Combine(this.byteWorker.SubArray(ranA, 12, 4), this.byteWorker.SubArray(actualRanB, 12, 4));
            byte[] C = this.byteWorker.Combine(A, B);
            Debug.WriteLine("Session Key:\t\t" + BitConverter.ToString(C).Replace("-", ""));
            Assert.AreEqual(expectedSessionKey, BitConverter.ToString(C).Replace("-", ""));
        }

        [TestMethod]
        public void Test_Decrypt()
        {
            string expected = "17AB67F130169FB3C012B2DD17985365";
            string uid = "04873ABA8D2C80";
            byte[] iv = this.hexConverter.Hex2Bytes("00000000000000000000000000000000");
            byte[] result = this.esKmsWebApi.Encrypt("2ICH3F000004A", iv, this.hexConverter.Hex2Bytes("0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80"));//"0104873ABA8D2C80494341534804873A9B330A45CCB51DDE66FDD7EABD400895"));//0104873ABA8D2C80494341534804873ABA8D2C80494341534804873ABA8D2C80"));
            Debug.WriteLine("DivKey:\t" + BitConverter.ToString(result));
            //Assert.AreEqual()
        }

        [TestCleanup]
        public void Clean()
        {

        }
    }
}
