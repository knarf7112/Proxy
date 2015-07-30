using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    /// <summary>
    /// 處理陣列的左旋,右旋,合併,取子陣列,Padding,轉置
    /// </summary>
    public class ByteWorker : IByteWorker
    {
        //Padding用的尾部指定值
        public static readonly byte FirstPadding = 0x80; // 0b10000000

        /// <summary>
        /// 陣列元素左旋幾次,若左旋超過陣列範圍的會移到陣列右邊去
        /// ex: {12,34,56,78,90,AB,CD,EF} ==左旋2次==> {56,78,90,AB,CD,EF,12,34}
        /// </summary>
        /// <param name="src">來源陣列</param>
        /// <param name="cnt">左旋次數</param>
        /// <returns>左旋後的新陣列</returns>
        public byte[] RotateLeft(byte[] src, int cnt)
        {
            byte[] result = new byte[src.Length];
            //  RotateLeft 2 byte 
            //  12,34,56,78,90,AB,CD,EF
            //        56,78,90,AB,CD,EF,12,34

            // 1.從左移的第一個元素開始設定 ex: 新陣列[0]=陣列[左移的Count] ...
            for (int i = 0, j = cnt; j < src.Length; i++, j++)
            {
                result[i] = src[j];
            }
            // 2.設定要移到右邊的元素
            for (int i = src.Length - 1, j = cnt - 1; j >= 0; i--, j--)
            {
                result[i] = src[j];
            }
            return result;
        }

        /// <summary>
        /// 陣列元素右旋幾次,若右旋陣列範圍的會移到陣列左邊去
        /// ex: {12,34,56,78,90,AB,CD,EF} ==右旋2次==> {CD,EF,12,34,56,78,90,AB}
        /// </summary>
        /// <param name="src">來源陣列</param>
        /// <param name="cnt">右旋次數</param>
        /// <returns>右旋後的新陣列</returns>
        public byte[] RotateRight(byte[] src, int cnt)
        {
            //  Rotate Right 2 byte 
            //        12,34,56,78,90,AB,CD,EF =>
            //  CD,EF,12,34,56,78,90,AB
            byte[] result = new byte[src.Length];

            for (int i = cnt - 1, j = src.Length - 1; i >= 0; i--, j--)
            {
                result[i] = src[j];
            }
            for (int i = cnt, j = 0; i < src.Length; i++, j++)
            {
                result[i] = src[j];
            }
            return result;
        }

        /// <summary>
        /// 合併兩個陣列產生一個新的合併陣列
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>合併的新陣列</returns>
        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        /// <summary>
        /// 合併2維陣列
        /// </summary>
        /// <param name="manyByteArr"></param>
        /// <returns></returns>
        public byte[] Combine(params byte[][] manyByteArr)
        {
            byte[] ret = new byte[manyByteArr.Sum(x => x.Length)];
            int offset = 0;
            //取得二維陣列裡面的每個陣列
            foreach (byte[] data in manyByteArr)
            {
                //複製到新的一維陣列裡
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        /// <summary>
        /// 取得來源陣列的指定子陣列
        /// </summary>
        /// <param name="src">來源陣列</param>
        /// <param name="beginIndex">指定的起始索引</param>
        /// <param name="length">子陣列長度</param>
        /// <returns></returns>
        public byte[] SubArray(byte[] src, int beginIndex, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = src[beginIndex++];
            }
            return result;
        }

        /// <summary>
        ///   op1 XOR op2  with bits of the byte array 
        /// </summary>
        /// <param name="op1">byte array1</param>
        /// <param name="op2">byte array2</param>
        /// <returns>Result of XOR</returns>
        public byte[] ExclusiveOr(byte[] op1, byte[] op2)
        {
            if (op1.Length != op2.Length)
            {
                throw new Exception("長度不符,無法運算");
            }
            byte[] result = new byte[op1.Length];
            for (int i = 0; i < op1.Length; i++)
            {
                result[i] = (byte)(op1[i] ^ op2[i]);
            }
            return result;
        }

        /// <summary>
        /// 將來源陣列補Padding
        /// 若長度整除16(BlockSize)則不作Padding,
        /// 若長度除16(BlockSize)有餘數則補上Padding =>{剩餘byte,0x80,0x00,0x00,0x00.....}
        /// </summary>
        /// <param name="srcBytes">來源陣列</param>
        /// <returns>Padding後的陣列</returns>
        public byte[] CMacPadding(byte[] srcBytes)
        {
            int blockCnt = srcBytes.Length / 16;
            int lastBytes = srcBytes.Length % 16;
            byte[] padded = null;
            if (lastBytes != 0)
            {
                blockCnt += 1;
                padded = new byte[blockCnt * 16];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
                padded[srcBytes.Length] = FirstPadding;
                for (int i = srcBytes.Length + 1; i < padded.Length; i++)
                {
                    padded[i] = 0;
                }
            }
            else
            {
                padded = new byte[srcBytes.Length];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
            }
            return padded;
        }

        /// <summary>
        /// 將來源陣列補Padding(有補的話,都補0)
        /// 若長度整除16(BlockSize)則不作Padding,
        /// 若長度除16(BlockSize)有餘數則補上Padding =>{剩餘byte,0x00,0x00,0x00,0x00.....}
        /// </summary>
        /// <param name="srcBytes"></param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public byte[] ZeroPadding(byte[] srcBytes, int blockSize)
        {
            int blockCnt = srcBytes.Length / blockSize;
            int lastBytes = srcBytes.Length % blockSize;
            byte[] padded = null;
            if (lastBytes != 0)
            {
                blockCnt += 1;
                padded = new byte[blockCnt * blockSize];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
                //padded[srcBytes.Length] = FirstPadding;
                for (int i = srcBytes.Length; i < padded.Length; i++)
                {
                    padded[i] = 0;
                }
            }
            else
            {
                padded = new byte[srcBytes.Length];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
            }
            return padded;
        }

        /// <summary>
        /// 比較陣列值是否相同
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <returns>相同/不同</returns>
        public bool AreEqual(byte[] op1, byte[] op2)
        {
            if (op1 == null)
            {
                if (op2 == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (op2 == null)
            {
                return false;
            }
            if (op1.Length != op2.Length)
            {
                return false;
            }
            for (int i = 0; i < op1.Length; i++)
            {
                if (op1[i] != op2[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 陣列反轉
        /// </summary>
        /// <param name="src">來源陣列</param>
        /// <returns>反轉的陣列</returns>
        public byte[] Reverse(byte[] src)
        {
            byte[] result = new byte[src.Length];
            for (int i = 0, j = src.Length - 1; i < src.Length; i++, j--)
            {
                result[j] = src[i];
            }
            return result;
        }

        /// <summary>
        /// 陣列值全部修改指定值
        /// </summary>
        /// <param name="src">來源陣列</param>
        /// <param name="pad">指定值</param>
        public void Fill(ref byte[] src, byte pad)
        {
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = pad;
            }
        }

        /// <summary>
        /// 產生陣列並全部指定值
        /// </summary>
        /// <param name="size">要產生的陣列大小</param>
        /// <param name="pad">指定的陣列值</param>
        /// <returns></returns>
        public byte[] Fill(int size, byte pad)
        {
            byte[] src = new byte[size];
            for (int i = 0; i < size; i++)
            {
                src[i] = pad;
            }
            return src;
        }
    }
}
