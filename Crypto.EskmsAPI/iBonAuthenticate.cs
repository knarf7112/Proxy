using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;
using System.Threading.Tasks;
using RandomGenerator;
using System.Diagnostics;

namespace Crypto.EskmsAPI
{
    public class iBonAuthenticate : IiBonAuthenticate
    {
        #region Field
        private IHexConverter hexConverter;

        private IByteWorker byteWorker;

        private IEsKmsWebApi esKmsWebApi;

        private ISymCryptor symCryptor;

        private ISessionKeyGenerator RandomACreater;

        public static readonly int ConstBlockSize = 16;

        private static readonly byte[] ConstZero = new byte[]
        {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };
        
        private static readonly byte[] AESDivConstant2 = new byte[] { 0x01 };
        
        private static readonly byte[] ICASH = new byte[] { 0x49, 0x43, 0x41, 0x53, 0x48 };

        private Random GenerateRanAIndex;

        private int macLength = ConstBlockSize;

        private byte[] iv = null;//initial vector
        private byte[] Kx = null;//divers Key(16 bytes)
        private byte[] RanA = null;
        #endregion

        #region Property
        
        #region Input
        /// <summary>
        /// ex:2ICH3F000032A
        /// </summary>
        public string Input_KeyLabel { private get; set; }

        /// <summary>
        /// 0x00
        /// </summary>
        public string Input_KeyVersion { private get; set; }

        /// <summary>
        /// 卡片UID(7 bytes) 
        /// </summary>
        public string Input_UID { private get; set; }

        /// <summary>
        /// UID組合完畢的資料:32 bytes(和卡片UID屬性二擇一輸入)
        /// byte[] { 01 + uid + ICASH + uid + ICASH + uid }
        /// </summary>
        public byte[] Input_BlobValue { private get; set; }

        /// <summary>
        /// E(RanB) => 16 bytes
        /// </summary>
        public string Input_Enc_RanB { private get; set; }

        /// <summary>
        /// 強制輸入的RanA
        /// 不輸入則從Dll產生隨機RanA值
        /// </summary>
        public byte[] Input_RanA { private get; set; }
        #endregion

        #region Output
        /// <summary>
        /// Random B
        /// </summary>
        public byte[] Output_RanB { get; private set; }

        /// <summary>
        /// iv = E(RanB)
        /// E( iv, (RanA || RanBRol8))
        /// </summary>
        public byte[] Output_Enc_RanAandRanBRol8 { get; private set; }

        /// <summary>
        /// iv = (E( iv, (RanA || RanBRol8))).SubArray(last 16 bytes)
        /// E( iv, RanARol8)
        /// </summary>
        public byte[] Output_Enc_IVandRanARol8 { get; private set; }

        /// <summary>
        /// Random A start index(16 bytes)
        /// </summary>
        public int Output_RandAStartIndex { get; private set; }

        /// <summary>
        /// Session Key = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        public byte[] Output_SessionKey { get; private set; }
        #endregion

        #endregion

        #region Constructor
        public iBonAuthenticate()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
            this.symCryptor = new SymCryptor();
            this.RandomACreater = new SessionKeyGenerator();
            this.GenerateRanAIndex = new Random(new Guid().GetHashCode());
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
                }
            };
        }
        #endregion

        /// <summary>
        /// Run (3 Pass) Authenticate Flow
        /// <param name="isGenSessionKey">是否產生SessionKey到SessionKey屬性內(預設:false)</param>
        /// </summary>
        public void StartAuthenticate(bool isGenSessionKey = false)
        {
            //1. 從KMS取得 diverse data,並產生Divers Key設到kx欄位(16 bytes)
            Generate_DivKey(this.Input_KeyLabel, this.Input_UID, iBonAuthenticate.ConstZero, this.Input_BlobValue);
            //2. 解密RanB,並將結果設到RanB屬性
            Generate_DecryptRanB();
            //3. 產生Enc(RanA + RanB(左旋 1 Byte))
            Generate_EncRanAandRanBRol8();
            //4. 產生Enc(iv,RanARol8)
            Generate_EncRanARol8();
            if (isGenSessionKey)
            {
                //5. 產生傳輸加解密用的Session Key
                Generate_SessionKey();
            }
        }

        /// <summary>
        /// 1.從KMS取得 diverse data,並將Divers Key設到kx欄位(16 bytes)
        /// </summary>
        /// <param name="keyLabel">KMS需要的程式命令參數</param>
        /// <param name="uid">卡片 UID</param>
        /// <param name="iv">KMS需要的initial vector</param>
        /// <param name="decrypted">blob value(null:表示使用AESDiv+uid+icash+uid+icash+uid)</param>
        private void Generate_DivKey(string keyLabel,string uid,byte[] iv,byte[] decrypted = null)
        {
            byte[] requestBlobValue = (decrypted == null) ? this.GetDiverseInput(uid) : decrypted;//diverse uid => 01+uid+icash+uid+icash+uid (total length: 32 bytes)
            Debug.WriteLine("1.開始Diverse Key:\n KeyLabel:" + keyLabel +
                                              "\n UID:" + uid + 
                                              "\n iv:" + BitConverter.ToString(iv).Replace("-", "") +
                                              "\n BlobValue:" + BitConverter.ToString(requestBlobValue).Replace("-", ""));
            byte[] responseData = this.esKmsWebApi.Encrypt(keyLabel, iv, requestBlobValue);//get diverse key
            this.Kx = this.byteWorker.SubArray(responseData, responseData.Length - this.macLength, this.macLength);// get mac from diverse key
            Debug.WriteLine("DivKey:" + BitConverter.ToString(this.Kx).Replace("-", ""));
        }

        /// <summary>
        /// 2.解密RanB,並將結果設到RanB屬性
        /// </summary>
        private void Generate_DecryptRanB()
        {
            if (this.Input_Enc_RanB == null || this.Input_Enc_RanB.Length != 32)
            {
                throw new Exception("Encrypt RanB is null or (hex string)length not 32");
            }

            Debug.WriteLine("2.開始解密RanB: Enc_RanB=" + this.Input_Enc_RanB);
            this.symCryptor.SetIV(iBonAuthenticate.ConstZero);
            this.symCryptor.SetKey(this.Kx);
            this.Output_RanB = this.symCryptor.Decrypt(this.hexConverter.Hex2Bytes(this.Input_Enc_RanB));
            Debug.WriteLine("Decrypted Random B:" + BitConverter.ToString(this.Output_RanB).Replace("-", ""));
        }

        /// <summary>
        /// 3. 產生Enc(RanA + RanB(左旋 1 Byte))
        /// </summary>
        private void Generate_EncRanAandRanBRol8()
        {
            Debug.WriteLine("3.開始產生Enc(RanA + RanB(左旋 1 Byte))");
            Generate_RanA(this.Input_RanA);
            byte[] ranBRol8 = this.byteWorker.RotateLeft(this.Output_RanB, 1);         //RanB左旋 1 byte
            byte[] ranA_ranBRol8 = this.byteWorker.Combine(this.RanA, ranBRol8);   //RandA + RanBRol8
            Debug.WriteLine("RandA + RanBRol8:" + BitConverter.ToString(ranA_ranBRol8).Replace("-", "") + 
                            "\n iv:" + this.Input_Enc_RanB);
            this.symCryptor.SetIV(this.hexConverter.Hex2Bytes(this.Input_Enc_RanB));  //iv = E(RanB)
            this.Output_Enc_RanAandRanBRol8 = this.symCryptor.Encrypt(ranA_ranBRol8);     //Enc(RandA + RanBRol8)
            Debug.WriteLine("E(RandA + RanBRol8):" + BitConverter.ToString(this.Output_Enc_RanAandRanBRol8).Replace("-", ""));
        }

        /// <summary>
        /// 4.產生Enc(iv,RanARol8)//iv =>使用EncRanAandRanBRol8尾部16bytes資料,並將RanA左旋8後加密
        /// </summary>
        private void Generate_EncRanARol8()
        {
            Debug.WriteLine("4.開始產生Enc(iv,RanARol8)");
            //iv = last 16 bytes of CApduData
            byte[] iv = this.byteWorker.SubArray(this.Output_Enc_RanAandRanBRol8, (this.Output_Enc_RanAandRanBRol8.Length - this.macLength), this.macLength);
            this.symCryptor.SetIV(iv);
            byte[] ranARol8 = this.byteWorker.RotateLeft(this.RanA, 1);
            this.Output_Enc_IVandRanARol8 = symCryptor.Encrypt(ranARol8);
            Debug.WriteLine("iv:" + BitConverter.ToString(iv).Replace("-", "") +
                            "\n RanARol8:" + BitConverter.ToString(ranARol8).Replace("-", "") +
                            "\n E(iv,RanARol8):" + BitConverter.ToString(this.Output_Enc_IVandRanARol8).Replace("-", ""));

        }

        /// <summary>
        /// 5. 輸出傳輸加解密用的Session Key
        /// SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        public byte[] Generate_SessionKey()
        {
            if (this.RanA != null && this.RanA.Length == ConstBlockSize && 
                this.Output_RanB != null && this.Output_RanB.Length == ConstBlockSize)
            {
                Debug.WriteLine("5.開始產生傳輸加解密用的Session Key");
                byte[] partA = this.byteWorker.Combine(this.byteWorker.SubArray(this.RanA, 0, 4), this.byteWorker.SubArray(this.Output_RanB, 0, 4));
                byte[] partB = this.byteWorker.Combine(this.byteWorker.SubArray(this.RanA, 12, 4), this.byteWorker.SubArray(this.Output_RanB, 12, 4));
                this.Output_SessionKey = this.byteWorker.Combine(partA, partB);
                Debug.WriteLine("RanA:" + BitConverter.ToString(this.RanA).Replace("-","") + 
                                "\n RanB:" + BitConverter.ToString(this.Output_RanB).Replace("-","") +
                                "\n SessionKey:" + BitConverter.ToString(this.Output_SessionKey).Replace("-",""));
                return this.Output_SessionKey;
            }
            else
            {
                throw new Exception("RanA or RanB is null or length not equals 16");
            }
        }

        /// <summary>
        /// 產出Random A 且設定RandomA Start Index
        /// <param name="ranA">強制使用的RanA</param>
        /// </summary>
        private void Generate_RanA(byte[] ranA = null)
        {
            if (this.Output_RandAStartIndex < 0)
            {
                throw new ArgumentOutOfRangeException("Random A Start Index: " + this.Output_RandAStartIndex + " < 0");
            }
            this.Output_RandAStartIndex = this.GenerateRanAIndex.Next(0, this.RandomACreater.GetTotalLength());
            this.RanA = (ranA == null) ? this.RandomACreater.GetRanA(this.Output_RandAStartIndex) : ranA;
            Debug.WriteLine("Get Random A(index:" + this.Output_RandAStartIndex + "):" + BitConverter.ToString(this.RanA).Replace("-", ""));
        }

        /// <summary>
        /// 卡片uid轉換成KMS的Blob需要的參數
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private byte[] GetDiverseInput(string uid)
        {
            byte[] uidBytes = this.hexConverter.Hex2Bytes(uid);
            return this.byteWorker.Combine
            (
                  AESDivConstant2
                , uidBytes
                , ICASH
                , uidBytes
                , ICASH
                , uidBytes
            );
        }
    }

}
