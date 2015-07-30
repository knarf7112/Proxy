using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;
using System.Diagnostics;

namespace Crypto
{
    /// <summary>
    /// NIST Special Publication 800-38B 
    /// Recommendation for Block Cipher Modes of Operation: 
    /// The CMAC(Cipher-based Message Authentication Code) Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
    /// </summary>
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

        public IHexConverter HexConverter { private get; set; }

        private byte[] macKey = null;//
        private int macLength = ConstBlockSize;
        private byte[] iv = ConstZero;
        private byte[] dataInput = null;
        private byte[] k1 = null;//左移1bit
        private byte[] k2 = null;//XOR iv後的結果

        /// <summary>
        /// 設定DataInput
        /// </summary>
        /// <param name="m">要加密的資料</param>
        public void DataInput(byte[] m)
        {
            this.dataInput = new byte[m.Length];
            Array.Copy(m, this.dataInput, m.Length);
        }

        /// <summary>
        /// 設定加密金鑰
        /// </summary>
        /// <param name="key">加密金鑰</param>
        public void SetMacKey(byte[] key)
        {
            this.macKey = new byte[key.Length];
            Array.Copy(key, this.macKey, key.Length);
            this.AesCryptor.SetKey(this.macKey);    //使用對稱式加解密
            this.k1 = this.k2 = null;
            //this.AesCryptor.SetIv(this.iv);
            //this.getSubKeys();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="macLength"></param>
        public void SetMacLength(int macLength)
        {
            //若key長度超過Block的Size
            if (macLength > ConstBlockSize)
            {
                throw new ArgumentOutOfRangeException("Max MAC size: " + ConstBlockSize);
            }
            this.macLength = macLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetMac()
        {
            byte[] fullMac = this.getFullMac(this.dataInput);
            if (this.macLength == ConstBlockSize)
            {
                return fullMac;
            }
            //byte[] macBytes = new byte[this.macLength];
            //Array.Copy(fullMac, 0, macBytes, 0, this.macLength);
            //return macBytes;

            return this.ByteWorker.SubArray(fullMac, 0, this.macLength);
        }

        /// <summary>
        /// Every odd bytes( strat from 0 ){1,3,5,7,9,11,13,15} from 16-byte standard CMAC
        /// 抽取資料陣列的奇數字節的資料
        /// </summary>
        /// <returns></returns>
        public byte[] GetOdd()
        {
            byte[] fullMac = this.getFullMac(this.dataInput);
            Console.WriteLine("Full Mac: " + this.HexConverter.Bytes2Hex(fullMac));
            byte[] resultBytes = new byte[this.macLength / 2];

            for (int i = 0, j = 1; j < fullMac.Length; i++, j+=2)
            {
                resultBytes[i] = fullMac[j];//取陣列的奇數字節資料 ex: [1],[3],[5],[7],[9],[11],...
            }
            return resultBytes;
        }

        /// <summary>
        /// 設定加解密時的初始值
        /// </summary>
        /// <param name="iv">initial vector</param>
        public void SetIV(byte[] iv)
        {
            this.iv = new byte[iv.Length];
            Array.Copy(iv, this.iv, iv.Length);
            //this.AesCryptor.SetIV(this.iv);
            this.k1 = this.k2 = null;
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
            //----------------------------------------------
            // 1.檢查k值
            if (this.k1 == null)
            {
                this.AesCryptor.SetIV(ConstZero);//設定演算物件的初始值
                this.getSubKeys();//設定k1,k2
            }
            //----------------------------------------------
            // 2.檢查資料是否需要補Padding(資料尾部處理)

            bool needPadding = false;
            int nBlock = inputData.Length / ConstBlockSize;//資料是否可被blockSize整除(沒剩資料)
            //資料不到一個BlockSize
            if (nBlock == 0)
            {
                nBlock = 1;//補一個block
                needPadding = true;
            }
            //資料超過一個BlockSize
            else
            {
                //資料除BlockSize後沒剩餘 , ex: 資料長度為2048 除16(BlockSize)後沒有餘數但也不補padding
                if (inputData.Length % ConstBlockSize == 0)
                {
                    needPadding = false;//不需要padding
                }
                else
                {
                    needPadding = true;
                    nBlock += 1;
                }
            }
            //----------------------------------------------
            // 3.開始處理資料尾部並依規格和k1或k2作XOR

            byte[] mLast;// = new byte[ConstBlockSize];//存資料尾部(最後一個block)
            //若不需要padding
            if (!needPadding)
            {
                // M_last := M_n XOR K1;//input資料取最後一個block和K1作XOR的結果
                //Array.Copy(inputData, inputData.Length - ConstBlockSize, mLast, 0, ConstBlockSize);
                mLast = this.BytesBitwiser.ExclusiveOr
                    (
                        this.ByteWorker.SubArray(inputData, inputData.Length - ConstBlockSize, ConstBlockSize),//input資料取最後一個Block
                        this.k1
                    );
            }
            else
            {
                //M_last := padding(M_n) XOR K2;//input資料作Padding後和k2作XOR的結果
                int lastBlockSize = inputData.Length % ConstBlockSize;//取得剩餘資料大小
                //Padding data
                mLast = this.BytesBitwiser.CMacPadding
                    (
                        this.ByteWorker.SubArray(inputData,inputData.Length - lastBlockSize,lastBlockSize) //取inputData最後一個block(資料不足1個block)
                    );
                Console.WriteLine("last block: " + this.HexConverter.Bytes2Hex(mLast));
                //do XOR with k2
                mLast = this.BytesBitwiser.ExclusiveOr(mLast, this.k2);
                Console.WriteLine("last block xor k2: " + this.HexConverter.Bytes2Hex(mLast));
            }
            //----------------------------------------------
            // 4.開始合併處理完的尾部資料

            //前半部(切割完整Block大小的input資料) + 後半部(剩餘input資料,依上面規格作完Padding且XOR完畢的)
            byte[] diverseData = this.ByteWorker.Combine
                (
                    this.ByteWorker.SubArray(inputData, 0, (nBlock - 1) * ConstBlockSize),
                    mLast
                );
            Console.WriteLine("diverseData: " + this.HexConverter.Bytes2Hex(diverseData));
            //----------------------------------------------
            // 5.設定IV後加密資料並回傳加密資料的最後一塊資料

            this.AesCryptor.SetIV(this.iv);
            byte[] result = this.AesCryptor.Encrypt(diverseData);//開始使用AES加密

            //回傳加密後的最後一塊(block)資料
            return this.ByteWorker.SubArray(result, result.Length - ConstBlockSize, ConstBlockSize);
        }

        /// <summary>
        /// 若無最高有效位數則陣列串連並左移1bit
        /// 若有最高有效位數則陣列串連並左移1bit後再和ConstRb作XOR
        /// 回傳結果陣列
        /// </summary>
        /// <param name="kx"></param>
        /// <returns></returns>
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
        /// 使用AesCryptor加密ConstZero取得K0並設定k1,k2
        /// </summary>
        private void getSubKeys()
        {
            //get k0
            //AES(K, 16-byte 0s).
            byte[] k0 = this.AesCryptor.Encrypt(ConstZero);
            Debug.WriteLine("K0: 0x" + this.HexConverter.Bytes2Hex(k0)); 
            //
            this.k1 = this.getNextSubKey(k0);//1.先左移 1 bit
            Debug.WriteLine("K1: 0x" + this.HexConverter.Bytes2Hex(this.k1));
            //
            this.k2 = this.getNextSubKey(k1);//2.再左移 1 bit 再和 ConstRb 作 XOR
            Debug.WriteLine("K2: 0x" + this.HexConverter.Bytes2Hex(this.k2)); 
        }
        #endregion
    }
}
