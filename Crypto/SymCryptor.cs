using System;
//
using System.IO;
using System.Security.Cryptography;

namespace Crypto
{
    /// <summary>
    /// 對稱式加解密
    /// Default for AES128 algorithm
    /// </summary>
    public class SymCryptor : ISymCryptor, IDisposable
    {
        #region Private Field
        private SymmetricAlgorithm _symmetricAlogithm;
        private int bufferSize = 4096;//initial default buffer
        #endregion

        #region Static member variable: ConstZero
        public static readonly byte[] ConstZero = new byte[]
        {
            0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0 
        };
        #endregion

        #region Constructor
        public SymCryptor():this("AES")
        {

        }

        public SymCryptor(string alg)
        {
            //設定演算法模式
            this.SetAlgorithm(alg);
        }

        public SymCryptor(string alg, string cipherMode)
        {
            //設定演算法模式
            this.SetAlgorithm(alg, this.GetCipherMode(cipherMode), PaddingMode.None);
        }
        #endregion

        #region Setting

        #region Property
        public int BufferSize
        {
            set { this.bufferSize = value; }
            get { return this.bufferSize; }
        }
        #endregion

        public void SetKey(byte[] key)
        {
            this._symmetricAlogithm.Key = key;
        }

        public void SetIV(byte[] iv)
        {
            this._symmetricAlogithm.IV = iv;
        }

        public void SetAlgorithm(string alg)
        {
            // keep padding none!
            this.SetAlgorithm(alg, CipherMode.CBC, PaddingMode.None);
        }

        public void SetAlgorithm(string alg, CipherMode cipherMode, PaddingMode paddingMode)
        {
            this._symmetricAlogithm = SymmetricAlgorithm.Create(alg);
            this._symmetricAlogithm.Mode = cipherMode;
            this._symmetricAlogithm.Padding = paddingMode;
        }
        #endregion

        #region Excute methods
        /// <summary>
        /// 加密資料
        /// </summary>
        /// <param name="data">要加密的原始資料</param>
        /// <returns>加密過的資料</returns>
        public byte[] Encrypt(byte[] data)
        {
            //建立對稱的加密子物件 (key和iv已設定,沒設定會跳異常)
            using (ICryptoTransform encryptor = this._symmetricAlogithm.CreateEncryptor())
            {
                return this.DoCrypt(data, encryptor);
            }
        }

        /// <summary>
        /// 解密加密過的資料
        /// </summary>
        /// <param name="encryptedData">加密過的資料</param>
        /// <returns>解密的資料</returns>
        public byte[] Decrypt(byte[] encryptedData)
        {
            //建立對稱的解密子物件 (key和iv已設定,沒設定會跳異常)
            using (ICryptoTransform decryptor = this._symmetricAlogithm.CreateDecryptor())
            {
                return this.DoCrypt(encryptedData, decryptor);
            }
        }
        #endregion

        public void Encrypt(Stream decryptedFile, Stream encryptedFile)
        {
            int readCnt = 0;
            byte[] buffer = new byte[this.BufferSize];

            using (ICryptoTransform encryptor = this._symmetricAlogithm.CreateEncryptor())
            {
                //將加密資料流包裝並注入加密演算子物件
                using (CryptoStream cryptoStream = new CryptoStream(encryptedFile, encryptor, CryptoStreamMode.Write))
                {
                    //
                    while ((readCnt = decryptedFile.Read(buffer,0,buffer.Length)) > 0)
                    {
                        cryptoStream.Write(buffer, 0, readCnt);
                    }
                }
            }
        }

        public void Decrypt(Stream encryptedFile, Stream decryptedFile)
        {
            int readCnt = 0;
            byte[] buffer = new byte[this.BufferSize];

            using (ICryptoTransform cryptor = this._symmetricAlogithm.CreateDecryptor())
            {
                using (CryptoStream cryptoStream = new CryptoStream(decryptedFile, cryptor, CryptoStreamMode.Write))
                {
                    while ((readCnt = encryptedFile.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cryptoStream.Write(buffer, 0, readCnt);
                    }
                }
            }
        }

        #region Dispose
        public void Dispose()
        {
            this._symmetricAlogithm.Clear();
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 只回傳兩種模式(EBC or CBC)
        /// </summary>
        /// <param name="cipherMode">cipherMode字串(指定要用來加密的區塊密碼模式)</param>
        /// <returns>EBC or CBC</returns>
        private CipherMode GetCipherMode(string cipherMode)
        {
            if (cipherMode.Equals("ECB"))
            {
                return CipherMode.ECB;
            }
            return CipherMode.CBC;
        }


        /// <summary>
        /// 共用的執行加解密流程,使用CryptoStream
        /// //ref: http://stackoverflow.com/questions/24903575/how-to-return-byte-when-decrypt-using-cryptostream-descryptoserviceprovider
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cryptor">密碼編譯轉換介面</param>
        /// <returns></returns>
        private byte[] DoCrypt(byte[] data, ICryptoTransform cryptor)
        {
            //使用memoryStream外層在包CryptoStream,並注入密碼編譯轉換介面
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(ms, cryptor, CryptoStreamMode.Write))
                {
                    //將加密或解密的資料寫入MemoryStream裡面,最後再輸出memoryStream裡面的資料
                    cryptoStream.Write(data, 0, data.Length);
                    //如果沒寫入,強制寫入
                    if (!cryptoStream.HasFlushedFinalBlock)
                    {
                        cryptoStream.FlushFinalBlock();
                    }
                }
                return ms.ToArray();
            }
        }
        #endregion
    }
}
