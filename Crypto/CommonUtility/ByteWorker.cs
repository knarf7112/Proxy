using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public class ByteWorker : IByteWorker
    {
        public static readonly byte FirstPadding = 0x80; // 0b10000000
        public byte[] RotateLeft(byte[] src, int cnt)
        {
            byte[] result = new byte[src.Length];
            //  RotateLeft 2 byte 
            //  1234567890ABCDEF
            //      567890ABCDEF1234
            for (int i = 0, j = cnt; j < src.Length; i++, j++)
            {
                result[i] = src[j];
            }
            for (int i = src.Length - 1, j = cnt - 1; j >= 0; i--, j--)
            {
                result[i] = src[j];
            }
            return result;
        }

        public byte[] RotateRight(byte[] src, int cnt)
        {
            //  Rotate Right 2 byte 
            //      1234567890ABCDEF
            //  CDEF1234567890AB
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

        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public byte[] Combine(params byte[][] manyByteArr)
        {
            byte[] ret = new byte[manyByteArr.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in manyByteArr)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

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

        public byte[] Reverse(byte[] src)
        {
            byte[] result = new byte[src.Length];
            for (int i = 0, j = src.Length - 1; i < src.Length; i++, j--)
            {
                result[j] = src[i];
            }
            return result;
        }

        public void Fill(ref byte[] src, byte pad)
        {
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = pad;
            }
        }

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
