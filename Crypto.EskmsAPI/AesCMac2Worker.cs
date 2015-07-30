using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// NIST Special Publication 800-38B 
    /// Recommendation for Block Cipher Modes of Operation: 
    /// The CMAC(Cipher-based Message Authentication Code) Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
    /// </summary>
    public class AesCMac2Worker : ICMac2Worker
    {
        #region Field
        //
        public static readonly int ConstBlockSize = 16;
        public static readonly byte FirstPadding = 0x80; // 0b10000000
        public static readonly byte[] ConstZero = new byte[]
        { 
            0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0 
        };
        public static readonly byte[] ConstRb = new byte[]
        { 
            0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0x87 
        };
        //public static const uint CKM_AES_ECB = 0x1081;
        //public static const uint CKM_AES_CBC = 0x1082;
        //

        //private string macKey;        
        private int macLength = ConstBlockSize;
        private byte[] iv = ConstZero;
        private byte[] dataInput = null;
        private byte[] k1 = null;
        private byte[] k2 = null;
        #endregion

        #region Property
        public IByteWorker ByteWorker { private get; set; }
        public IBytesBitwiser BytesBitwiser { private get; set; }
        public HexConverter HexConverter { private get; set; }
        public IEsKmsWebApi EsKmsWebApi { private get; set; }

        public ISymCryptor SymCryptor { private get; set; }
        #endregion

        #region Constructor 
        public AesCMac2Worker()
        {
            this.ByteWorker = new ByteWorker();
            this.BytesBitwiser = new BytesBitwiser();
            this.HexConverter = new HexConverter();
            this.EsKmsWebApi = new EsKmsWebApi();
            this.SymCryptor = new SymCryptor();
        }
        #endregion
        public void SetIV(byte[] iv)
        {
            
        }
        /// <summary>
        /// 要加解密的資料寫入
        /// </summary>
        /// <param name="m"></param>
        public void DataInput(byte[] m)
        {
            this.dataInput = new byte[m.Length];
            Array.Copy(m, this.dataInput, m.Length);
        }

        public void SetMacKey(string keyLabel)
        {
            
        }

        public void SetMacLength(int macLength)
        {
            throw new NotImplementedException();
        }

        public byte[] GetMAC()
        {
            throw new NotImplementedException();
        }

        public byte[] GetOdd()
        {
            throw new NotImplementedException();
        }
    }
}
