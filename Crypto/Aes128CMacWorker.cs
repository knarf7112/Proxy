using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;

namespace Crypto
{
    public class Aes128CMacWorker : ICMacWorker
    {

        #region Static Field
        public static readonly int ConstBlockSize = 16;//切割資料用的基準值
        public static readonly byte FirstPadding = 0x80;
        public static readonly byte[] ConstZero = SymCryptor.ConstZero;//16個 0
        public static readonly byte[] ConstRb ={
                                                   0, 0, 0, 0,
                                                   0, 0, 0, 0,
                                                   0, 0, 0, 0,
                                                   0, 0, 0, 0x87
                                               };//規格規定最後一個為(1000 0111)

        #endregion

        public IByteWorker ByteWorker { private get; set; }

        

        public void DataInput(byte[] m)
        {
            throw new NotImplementedException();
        }

        public void SetMacKey(byte[] key)
        {
            throw new NotImplementedException();
        }

        public void SetMacLength(int macLength)
        {
            throw new NotImplementedException();
        }

        public byte[] GetMac()
        {
            throw new NotImplementedException();
        }

        public byte[] GetOdd()
        {
            throw new NotImplementedException();
        }

        public void SetIV(byte[] iv)
        {
            throw new NotImplementedException();
        }
    }
}
