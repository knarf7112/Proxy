using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;
using System.Threading.Tasks;
using RandomGenerator;

namespace Crypto.EskmsAPI
{
    public class iBonAuthenticate
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

        private Random GenerateRamAIndex;

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
        public string KeyLabel { private get; set; }

        /// <summary>
        /// 0x00
        /// </summary>
        public string KeyVersion { private get; set; }

        /// <summary>
        /// 卡片UID(7 bytes) 
        /// </summary>
        public string UID { private get; set; }

        /// <summary>
        /// E(RanB) => 32 bytes
        /// </summary>
        public string Enc_RanB { private get; set; }
        #endregion

        #region Output
        public byte[] RanB { get; private set; }

        public byte[] Enc_RanAandRanBRol8 { get; private set; }

        public byte[] Enc_IVandRanARol8 { get; private set; }

        public int RandAStartIndex { get; private set; }

        public byte[] SessionKey { get; private set; }
        #endregion

        #endregion

        #region Constructor
        public iBonAuthenticate()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
            this.symCryptor = new SymCryptor();
            this.RandomACreater = new SessionKeyGenerator();
            this.GenerateRamAIndex = new Random(new Guid().GetHashCode());
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


        public void StartAuthenticate()
        {
            Generate_DivKey(this.KeyLabel, this.UID, iBonAuthenticate.ConstZero);
            Generate_DecryptRanB();

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
            byte[] responseData = this.esKmsWebApi.Encrypt(keyLabel, iv, requestBlobValue);//get diverse key
            this.Kx = this.byteWorker.SubArray(responseData, responseData.Length - this.macLength, this.macLength);// get mac from diverse key
        }

        /// <summary>
        /// 2.解密RanB,並將結果設到RanB屬性
        /// </summary>
        private void Generate_DecryptRanB()
        {
            this.symCryptor.SetIV(iBonAuthenticate.ConstZero);
            this.symCryptor.SetKey(this.Kx);
            this.RanB = this.symCryptor.Decrypt(this.hexConverter.Hex2Bytes(this.Enc_RanB));
        }

        /// <summary>
        /// 3. 產生Enc(RanA + RanB(左旋 1 Byte))
        /// </summary>
        private void Generate_EncRanAandRanBRol8()
        {
            Generate_RanA();
            byte[] ranBRol8 = this.byteWorker.RotateLeft(this.RanB, 1);         //RanB左旋 1 byte
            byte[] ranA_Brol8 = this.byteWorker.Combine(this.RanA, ranBRol8);   //RandA + RanBRol8
            this.symCryptor.SetIV(this.hexConverter.Hex2Bytes(this.Enc_RanB));  //iv = E(RanB)
            this.Enc_RanAandRanBRol8 = this.symCryptor.Encrypt(ranA_Brol8);     //Enc(RandA + RanBRol8)
        }

        /// <summary>
        /// 4.產生Enc(iv,RanARol8)//iv =>使用EncRanAandRanBRol8尾部16bytes資料,並將RanA左旋8後加密
        /// </summary>
        private void Generate_EncRanARol8()
        {
            //iv = last 16 bytes of CApduData
            byte[] iv = this.byteWorker.SubArray(this.Enc_RanAandRanBRol8, 16, 16);
            this.symCryptor.SetIV(iv);
            byte[] ranARol8 = this.byteWorker.RotateLeft(this.RanA, 1);
            this.Enc_IVandRanARol8 = symCryptor.Encrypt(ranARol8);
        }

        /// <summary>
        /// 5. 輸出傳輸加解密用的Session Key
        /// SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        public byte[] Generate_SessionKey()
        {
            if (this.RanA != null && this.RanA.Length == ConstBlockSize && 
                this.RanB != null && this.RanB.Length == ConstBlockSize)
            {
                byte[] partA = this.byteWorker.Combine(this.byteWorker.SubArray(this.RanA, 0, 4), this.byteWorker.SubArray(this.RanB, 0, 4));
                byte[] partB = this.byteWorker.Combine(this.byteWorker.SubArray(this.RanA, 12, 4), this.byteWorker.SubArray(this.RanB, 12, 4));
                this.SessionKey = this.byteWorker.Combine(partA, partB);
                
                return this.SessionKey;
            }
            else
            {
                throw new Exception("RanA or RanB is null or length not equals 16");
            }
        }

        /// <summary>
        /// 產出Random A 且設定RandomA Start Index
        /// </summary>
        private void Generate_RanA()
        {
            this.RandAStartIndex = this.GenerateRamAIndex.Next(0, this.RandomACreater.GetTotalLength());
            this.RanA = this.RandomACreater.GetRanA(this.RandAStartIndex);
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
