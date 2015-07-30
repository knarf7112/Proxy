using System;
//
using Crypto.CommonUtility;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// By NXP AN10992, AES128CMAC
    /// 用來產生衍生key
    /// </summary>
    public class Icash2KeyDeriver //: IKey2Deriver
    {

        #region Field
        private static readonly byte[] AESDivConstant2 = new byte[] { 0x01 };//InputData的頭
        private static readonly byte[] ICASH = new byte[] { 0x49, 0x43, 0x41, 0x53, 0x48 };//InputData摻雜的料
        #endregion

        #region Property
        public ICMacWorker AesCMac2Worker { private get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IByteWorker ByteWorker { private get; set; }

        /// <summary>
        /// 對稱式加解密物件
        /// </summary>
        public ISymCryptor AesCryptor { private get; set; }

        /// <summary>
        /// 處理 Hex <=> Bytes
        /// </summary>
        public IHexConverter HexConverter { private get; set; }

        /// <summary>
        /// 衍生Key
        /// </summary>
        private byte[] derivedKey { get; set; }
        #endregion

        public void DiversInput(byte[] m)
        {
            throw new NotImplementedException();
        }

        public void SetSeedKey(byte[] key)
        {
            throw new NotImplementedException();
        }

        public byte[] GetDerivedKey()
        {
            throw new NotImplementedException();
        }
    }
}
