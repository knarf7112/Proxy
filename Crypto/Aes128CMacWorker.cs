using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;

namespace Crypto
{
    /// <summary>
    /// NIST Special Publication 800-38B 
    /// Recommendation for Block Cipher Modes of Operation: 
    /// The CMAC(Cipher-based Message Authentication Code) Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
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

        public IBytesBitwiser BytesBitwiser { private get; set; }

        public ISymCryptor AesCryptor { private get; set; }

        public HexConverter HexConverter { private get; set; }

        private byte[] macKey = null;//
        private int macLength = ConstBlockSize;
        private byte[] iv = ConstZero;
        private byte[] dataInput = null;
        private byte[] k1 = null;//左移1bit
        private byte[] k2 = null;//XOR iv後的結果

        /// <summary>
        /// 設定DataInput
        /// </summary>
        /// <param name="m"></param>
        public void DataInput(byte[] m)
        {
            this.dataInput = new byte[m.Length];
            Array.Copy(m, this.dataInput, m.Length);
        }

        public void SetMacKey(byte[] key)
        {
            this.macKey = new byte[key.Length];
            Array.Copy(key, this.macKey, key.Length);
            this.AesCryptor.SetKey(this.macKey);    //使用對稱式加解密
            this.k1 = this.k2 = null;
            //this.AesCryptor.SetIv(this.iv);
            //this.getSubKeys();
        }

        public void SetMacLength(int macLength)
        {
            //若key長度超過Block的Size
            if (macLength > ConstBlockSize)
            {
                throw new ArgumentOutOfRangeException("Max MAC size: " + ConstBlockSize);
            }
            this.macLength = macLength;
        }

        public byte[] GetMac()
        {
            //byte[] fullMac = this.
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
        #region Private 
        /*
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +                   Algorithm AES-CMAC                              +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +                                                                   +
   +   Input    : K    ( 128-bit key )                                 +
   +            : M    ( message to be authenticated )                 +
   +            : len  ( length of the message in octets )             +
   +   Output   : T    ( message authentication code )                 +
   +                                                                   +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +   Constants: const_Zero is 0x00000000000000000000000000000000     +
   +              const_Bsize is 16                                    +
   +                                                                   +
   +   Variables: K1, K2 for 128-bit subkeys                           +
   +              M_i is the i-th block (i=1..ceil(len/const_Bsize))   +
   +              M_last is the last block xor-ed with K1 or K2        +
   +              n      for number of blocks to be processed          +
   +              r      for number of octets of last block            +
   +         needPadding for denoting if last block is complete or not +
   +                                                                   +
   +   Step 1.  (K1,K2) := Generate_Subkey(K);                         +
   +   Step 2.  n := ceil(len/const_Bsize);                            +
   +   Step 3.  if n = 0                                               +
   +            then                                                   +
   +                 n := 1;                                           +
   +                 needPadding := true;                              +
   +            else                                                   +
   +                 if len mod const_Bsize is 0                       +
   +                 then needPadding := false;                        +
   +                 else needPadding := true;                         +
   +                                                                   +
   +   Step 4.  if needPadding is false                                +
   +            then M_last := M_n XOR K1;                             +
   +            else M_last := padding(M_n) XOR K2;                    +
   +   Step 5.  X := const_Zero;                                       +
   +   Step 6.  for i := 1 to n-1 do                                   +
   +                begin                                              +
   +                  Y := X XOR M_i;                                  +
   +                  X := AES-128(K,Y);                               +
   +                end                                                +
   +            Y := M_last XOR X;                                     +
   +            T := AES-128(K,Y);                                     +
   +   Step 7.  return T;                                              +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
         */
        private byte[] getFullMac(byte[] inputData)
        {
            if (this.k1 == null)
            {
                this.AesCryptor.SetIV(ConstZero);//設定演算物件的初始值
                this.getSubKeys();//設定k1,k2
            }
            bool needPadding = false;
            int nBlock = inputData.Length / ConstBlockSize;//資料是否可被blockSize整除(沒剩資料)
            if (nBlock == 0)
            {
                nBlock = 1;
                needPadding = true;
            }
            else
            {
                //toDO...................
            }
            return null;
        }


        private byte[] getNextSubKey(byte[] kx)
        {
            byte[] k;
            //若key不存在最高有效位元
            if (!this.BytesBitwiser.MsbOne(kx))
            {
                //key 左移 1 bit
                k = this.BytesBitwiser.ShiftLeft(kx, 1);
            }
            else
            {
                //key 左移 1 bit 再和 ConstRb 作 XOR
                k = this.BytesBitwiser.ExclusiveOr(this.BytesBitwiser.ShiftLeft(kx, 1), ConstRb);
            }
            return k;
        }

        /// <summary>
        /// 使用AesCryptor加密ConstZero設定K0,k1,k2
        /// </summary>
        private void getSubKeys()
        {
            //get k0
            //AES(K, 16-byte 0s).
            byte[] k0 = this.AesCryptor.Encrypt(ConstZero);
            Console.WriteLine("K0: 0x" + this.HexConverter.Bytes2Hex(k0)); 
            //
            this.k1 = this.getNextSubKey(k0);//1.先左移 1 bit
            Console.WriteLine("K1: 0x" + this.HexConverter.Bytes2Hex(this.k1));
            //
            this.k2 = this.getNextSubKey(k1);//2.再左移 1 bit 再和 ConstRb 作 XOR
            Console.WriteLine("K2: 0x" + this.HexConverter.Bytes2Hex(this.k2)); 
        }
        #endregion
    }
}
