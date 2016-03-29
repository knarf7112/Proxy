using System;
using System.Collections.Generic;
//KMS 2.0 WebApi
using Crypto.EskmsAPI;
//Load kms config
using System.Xml.Linq;
//Crypto Utility
using Crypto.CommonUtility;

namespace Crypto.ZMK
{
    /// <summary>
    /// ZMK_data Handler
    /// </summary>
    public class ZMK_Manager : IZMK_Manager
    {
        private static readonly int KEYLEBAL_LENGTH = 13;
        //private static readonly int IV_LENGTH = 16;//EsKmsWebApi作ECB時,不需要iv

        #region Private Properties
        private static IDictionary<string, string> dicKmsLoginConfig;
        private string _keyLabel;
        private byte[] _iv;
        private IEsKmsWebApi _KMS_WebApi;
        #endregion

        public ZMK_Manager(string keyLabel, byte[] iv)
        {
            if (String.IsNullOrEmpty(keyLabel) || !keyLabel.Length.Equals(KEYLEBAL_LENGTH))
                throw new ArgumentOutOfRangeException("keyLabel is null or length not equals " + KEYLEBAL_LENGTH);
            //if (iv == null || !iv.Length.Equals(IV_LENGTH))
            //    throw new ArgumentOutOfRangeException("iv is null or length not equals " + IV_LENGTH);
            if (dicKmsLoginConfig == null)
            {
                LoadKMSConfig(@"EsKmsWebApiConfig.xml");
            }
            this._keyLabel = keyLabel;
            this._iv = iv;
            this._KMS_WebApi = new EsKmsWebApi()
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
        }

        #region Public Method

        /// <summary>
        /// use KMS 2.0 Libs encrypt ZMK_Data(Random:16 bytes)
        /// </summary>
        /// <returns>Encrypted ZMK data:16 bytes</returns>
        public byte[] Generate_ZMK_Data(out byte[] encZMK_data)
        {
            byte[] randomZMK_Data = this.Gen_Rnd16();
            encZMK_data = this._KMS_WebApi.Encrypt(this._keyLabel, this._iv, randomZMK_Data);
            return randomZMK_Data;
        }

        /// <summary>
        /// input ZMK_Data(random array) and use KMS 2.0 Libs encrypt ZMK_Data(Random:16 bytes)
        /// </summary>
        /// <param name="zmk_data">ZMK_Data(random array)</param>
        /// <returns>true:encrypt ZMK_Data/false:error</returns>
        public byte[] Get_EncZMK_Data(byte[] zmk_data)
        {
            if (zmk_data.Length != 16)
            {
                return null;
            }
            return this._KMS_WebApi.Encrypt(this._keyLabel, this._iv, zmk_data);
        }

        /// <summary>
        /// use KMS 2.0 Libs decrypt Encrypt_ZMK_Data(16 bytes)
        /// </summary>
        /// <param name="encryptedData">encrypted ZMK data</param>
        /// <returns>Decrypted ZMK data:16 bytes</returns>
        public byte[] GetDecrypt_ZMK_Data(byte[] encryptedData)
        {
            return this._KMS_WebApi.Decrypt(this._keyLabel, this._iv, encryptedData);
        }

        /// <summary>
        /// Get Xor data when input data1 and data2
        /// </summary>
        /// <param name="data1">data1:16 bytes</param>
        /// <param name="data2">data2:16 bytes</param>
        /// <returns>Xor data:16 bytes</returns>
        public byte[] Get_XOR_data(byte[] data1, byte[] data2)
        {
            if (data1.Length != data2.Length)
                throw new ArgumentException("data1 length must be equals data2 length");
            byte[] result = new byte[data1.Length];
            for (int i = 0; i < data1.Length; i++)
            {
                result[i] = (byte)(data1[i] ^ data2[i]);
            }

            return result;
        }

        /// <summary>
        /// Generate A part and B part data when input ZMK_Data
        /// </summary>
        /// <param name="zmk_data"></param>
        /// <param name="a_part"></param>
        /// <param name="b_part"></param>
        public bool Get_AB_Part(byte[] zmk_data, out byte[] a_part, out byte[] b_part)
        {
            try
            {
                a_part = this.Gen_Rnd16();
                b_part = this.Get_XOR_data(zmk_data, a_part);
                return true;
            }
            catch (Exception ex)
            {
                a_part = null;
                b_part = null;
                return false;
            }
        }
        /// <summary>
        /// get guid array : 16 bytes
        /// </summary>
        /// <returns>radom data:16 bytes</returns>
        public virtual byte[] Gen_Rnd16()
        {
            //generate ZMK_Data
            return Guid.NewGuid().ToByteArray();
        }
        #endregion

        #region Protected Method
        /// <summary>
        /// 取得連入KMS的設定檔
        /// </summary>
        /// <param name="fileName"></param>
        protected virtual void LoadKMSConfig(string fileName)
        {
            dicKmsLoginConfig = new Dictionary<string, string>();
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
        #endregion

        #region Private Method

        #endregion
    }
}
