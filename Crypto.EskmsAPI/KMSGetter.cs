using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;
using System.Xml.Linq;
using System.Diagnostics;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// 向KMS取DiversKey的
    /// </summary>
    public class KMSGetter : IKMSGetter
    {
        #region Field
        private IHexConverter hexConverter;

        private IByteWorker byteWorker;

        private IEsKmsWebApi esKmsWebApi;

        private AesCMac2Worker aesCMac2Worker;

        private ISymCryptor symCryptor;

        /// <summary>
        /// Block Size
        /// </summary>
        public static readonly int ConstBlockSize = 16;

        private static readonly byte[] ConstZero = new byte[]
        {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        private static readonly byte[] AESDivConstant2 = new byte[] { 0x01 };

        private static readonly byte[] ICASH = new byte[] { 0x49, 0x43, 0x41, 0x53, 0x48 };

        private int macLength = ConstBlockSize;

        //private byte[] iv = null;//initial vector
        private byte[] Kx = null;//divers Key(16 bytes)

        /// <summary>
        /// 登入KMS的帳號密碼設定檔
        /// </summary>
        private static IDictionary<string, string> dicKmsLoginConfig;
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
        /// 裝置ID(16 bytes => 32 hexString)
        /// </summary>
        public string Input_DeviceID { set; get; }
        #endregion

        #region Output
        
        #endregion

        #endregion

        #region Constructor
        public KMSGetter()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
            this.symCryptor = new SymCryptor();
            if (dicKmsLoginConfig == null)
            {
                LoadXmlConfig(@"EsKmsWebApiConfig.xml");
            }
            this.esKmsWebApi = new EsKmsWebApi()
            {
                Url = dicKmsLoginConfig["Url"],
                //"http://10.27.68.163:8080/eGATEsKMS/interface",
                //"http://127.0.0.1:8081/eGATEsKMS/interface",
                AppCode = dicKmsLoginConfig["AppCode"],
                //"APP_001",
                AuthCode = dicKmsLoginConfig["AuthCode"],
                //"12345678",
                AppName = dicKmsLoginConfig["AppName"],
                //"icash2Test",
                HttpMethod = dicKmsLoginConfig["HttpMethod"],
                //"POST",
                HexConverter = new HexConverter(),
                HashWorker = new HashWorker()
                {
                    HashAlg = "SHA1",
                    HexConverter = new HexConverter()
                }
            };

            this.aesCMac2Worker = new AesCMac2Worker(this.esKmsWebApi);
        }
        #endregion

        /// <summary>
        /// 取得Divers Key,異常則回傳null
        /// </summary>
        /// <returns>byte[]/null</returns>
        public virtual byte[] GetDiversKey()
        {
            try
            {
                this.Generate_DivKey(this.Input_KeyLabel, this.Input_UID, KMSGetter.ConstZero, this.Input_BlobValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GetDiversKey] Error:" + ex.StackTrace);
                return null;
            }
            return this.Kx;
        }

        /// <summary>
        /// 1.從KMS取得 diverse data,並將Divers Key設到kx欄位(16 bytes)
        /// </summary>
        /// <param name="keyLabel">KMS需要的程式命令參數</param>
        /// <param name="uid">卡片 UID</param>
        /// <param name="iv">KMS需要的initial vector</param>
        /// <param name="decrypted">blob value(null:表示使用AESDiv+uid+icash+uid+icash+uid)</param>
        protected void Generate_DivKey(string keyLabel, string uid, byte[] iv, byte[] decrypted = null)
        {
            byte[] requestBlobValue = (decrypted == null) ? this.GetDiverseInput(uid) : decrypted;//diverse uid => 01+uid+icash+uid+icash+uid (total length: 32 bytes)
            Debug.WriteLine("1.開始Diverse Key:\n KeyLabel:" + keyLabel +
                                              "\n UID:" + uid +
                                              "\n iv:" + BitConverter.ToString(iv).Replace("-", "") +
                                              "\n BlobValue:" + BitConverter.ToString(requestBlobValue).Replace("-", ""));
            //byte[] responseData = this.esKmsWebApi.Encrypt(keyLabel, iv, requestBlobValue);//get diverse key//(錯誤的)舊的直接傳,01+uid+ICASH+uid+ICASH+uid
            this.aesCMac2Worker.SetIv(AesCMac2Worker.ConstZero);
            this.aesCMac2Worker.DataInput(requestBlobValue);
            this.aesCMac2Worker.SetMacKey(keyLabel);
            byte[] responseData = this.aesCMac2Worker.GetMac();
            this.Kx = this.byteWorker.SubArray(responseData, responseData.Length - this.macLength, this.macLength);// get mac from diverse key
            Debug.WriteLine("DivKey:" + BitConverter.ToString(this.Kx).Replace("-", ""));
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

        /// <summary>
        /// 取得連入KMS的設定檔
        /// </summary>
        /// <param name="fileName"></param>
        protected void LoadXmlConfig(string fileName)
        {
            KMSGetter.dicKmsLoginConfig = new Dictionary<string, string>();
            string fileFullPath = AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + fileName;
            XDocument doc = XDocument.Load(fileFullPath);
            XElement root = doc.Root;
            string url = root.Element("Url").Value;
            string appCode = root.Element("AppCode").Value;
            string authCode = root.Element("AuthCode").Value;
            string appName = root.Element("AppName").Value;
            string httpMethod = root.Element("HttpMethod").Value;
            dicKmsLoginConfig.Add("Url", url);
            dicKmsLoginConfig.Add("AppCode", appCode);
            dicKmsLoginConfig.Add("AuthCode", authCode);
            dicKmsLoginConfig.Add("AppName", appName);
            dicKmsLoginConfig.Add("HttpMethod", httpMethod);
        }
    }
}
