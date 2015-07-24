using System;
//
using System.Security.Cryptography;
using Crypto.CommonUtility;
using System.IO;

namespace Crypto
{
    public class HashWorker : IHashWorker
    {
        #region Field
        //使用hash基底工具
        private HashAlgorithm hashAlg;

        private string hashAlgStr;
        #endregion

        #region Property
        /// <summary>
        /// 設定要使用哪種hash演算法
        /// SHA1/SHA256/MD5
        /// </summary>
        public string HashAlg
        {
            set
            {
                this.hashAlgStr = value;
                switch (value.ToUpper())
                {
                    case "SHA1":
                        this.hashAlg = SHA1.Create();
                        break;
                    case "MD5":
                        this.hashAlg = MD5.Create();
                        break;
                    case "SHA256":
                        this.hashAlg = SHA256.Create();
                        break;
                    default:
                        this.hashAlgStr = "SHA1";
                        this.hashAlg = SHA1.Create();
                        break;
                }
            }
        }

        public HexConverter HexConverter { get; set; }
        #endregion

        #region Constructor
        public HashWorker():this("SHA1")
        {

        }

        public HashWorker(string hashAlg)
        {
            this.HashAlg = hashAlg;
            this.HexConverter = new HexConverter();
        }
        #endregion

        #region Public Method
        /// <summary>
        /// 將資料陣列作Hash
        /// </summary>
        /// <param name="decrypted"></param>
        /// <returns></returns>
        public byte[] ComputeHash(byte[] decrypted)
        {
            return this.hashAlg.ComputeHash(decrypted);
        }

        /// <summary>
        /// 將資料流內的資料作Hash後,輸出Hash陣列
        /// </summary>
        /// <param name="stream">要hash的資料流</param>
        /// <returns>Hash陣列</returns>
        public byte[] ComputeHash(Stream stream)
        {
            return this.hashAlg.ComputeHash(stream);
        }

        /// <summary>
        /// 將資料陣列先hash產生hash陣列後,再將hash陣列轉hex字串
        /// </summary>
        /// <param name="decrypted">資料來源</param>
        /// <returns>Hashed HexString</returns>
        public string Hash2Hex(byte[] decrypted)
        {
            return this.HexConverter.Bytes2Hex(this.ComputeHash(decrypted));
        }

        /// <summary>
        /// 將資料陣列先hash產生hash陣列後,再將hash陣列轉Base64字串
        /// </summary>
        /// <param name="decrypted">資料來源</param>
        /// <returns>Hashed Base64String</returns>
        public string Hash2Base64(byte[] decrypted)
        {
            return Convert.ToBase64String(this.ComputeHash(decrypted));
        }

        /// <summary>
        /// 初始化Hash演算物件
        /// </summary>
        public void Initialize()
        {
            this.hashAlg.Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <param name="inputOffset"></param>
        /// <param name="inputCount"></param>
        /// <returns></returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = null;
            if (this.hashAlg.CanReuseTransform && this.hashAlg.CanTransformMultipleBlocks)
            {
                outputBuffer = new byte[inputCount];
                return this.hashAlg.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            }
            else
            {               
                throw new Exception("Can't operate TransfromBlock ...");
            }
        }

        public byte[] GetHash()
        {
            if (this.hashAlg.CanReuseTransform && this.hashAlg.CanTransformMultipleBlocks)
            {
                byte[] tmp = new byte[8];
                this.hashAlg.TransformFinalBlock(tmp, 0, 0);
            }
            else
            {
                throw new Exception("Can't get Hash from Multiple Blocks...");
            }
            return this.hashAlg.Hash;
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <param name="inputOffset"></param>
        /// <param name="inputCount"></param>
        /// <returns></returns>
        private byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (this.hashAlg.CanReuseTransform && this.hashAlg.CanTransformMultipleBlocks)
            {
                return this.hashAlg.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
            }
            else
            {
                throw new Exception("Can't operate TransformFinalBlock...");
            }
        }
        #endregion
    }
}
