using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public class BytesBitwiser : IBytesBitwiser
    {
        private static readonly byte MSB_BYTE = (byte)0x80;
        private static readonly byte LSB_BYTE = (byte)0x01;

        /// <summary>
        /// 檢查陣列第一個元素是否存在最高有效位數
        /// [1xxx xxxx] => true
        /// [0xxx xxxx] => false
        /// </summary>
        /// <param name="bytes">來源陣列</param>
        /// <returns>是否存在最高有效位數</returns>
        public bool MsbOne(byte[] bytes)
        {
            return (bytes[0] & MSB_BYTE) == MSB_BYTE;
        }

        /// <summary>
        /// 將Byte Array所有元素向左移 n bit
        /// ex:
        /// {129,130}=>{ 10000001,10000010}
        /// 左移1bit,會將第1個bit丟掉
        /// {00000011,00000100} => {3,4}
        /// </summary>
        /// <param name="srcBytes">要左移的陣列來源</param>
        /// <param name="shiftCnt">要左移幾bit</param>
        /// <returns>左移後的陣列</returns>
        public byte[] ShiftLeft(byte[] srcBytes, int shiftCnt)
        {
            byte[] bytes = new byte[srcBytes.Length];
            //先複製原始陣列
            Array.Copy(srcBytes,bytes,srcBytes.Length);
            //看要向左位移幾次 ex: 1010 0110 ==左移2次==> 前兩位(10)被丟掉 ==> 1001 1000 
            for(int i = 0; i < shiftCnt; i++)
            {
                //開始向左位移所有陣列元素
                this.shiftLeft(ref bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Shifts the bits in an array of bytes to the left one by one.
        /// ex: 
        /// {90,254,1} ==轉二進位==> {0101 1010(90),1111 1110(254),0000 0001(1)}
        /// {0101 1010(90),1111 1110(254),0000 0001(1)} ==向左位移一位==> {1011 0101(181),1111 1100(252),0000 0010(2)}
        /// 如果陣列的第一個元素最高位數[1xxx xxxx]有包含1則要位移到陣列最後一位元素的最右側
        /// </summary>
        /// <param name="bytes">The byte array to shift.</param>
        /// <returns>回傳陣列第一個元素的最高有效位數存歿 true(1):false(0)</returns>
        private bool shiftLeft(ref byte[] bytes)
        {
            bool leftMostCarryFlag = false;//用來暫存陣列第一個元素的最高有效位數(如果有1的話)

            // Iterate through the elements of the array from left to right.
            // 向左位移每個陣列元素,並要確認每個元素是否存在最高有效位數,如果存在要OR到上個陣列元素
            for (int index = 0; index < bytes.Length; index++)
            {
                // If the leftmost bit of the current byte is 1 then we have a carry.
                bool carryFlag = (bytes[index] & MSB_BYTE) > 0;//檢查是否存在最高有效位數 => [1xxx xxxx]
                //檢查是否為第一個陣列元素
                if (index > 0)
                {
                    //如果存在最高有效位數
                    if (carryFlag)
                    {
                        // Apply the carry to the rightmost bit of the current bytes neighbor to the left.
                        //將此次的最高有效位數接到上一個元素的最右側(此次最高有效位數[1000 0000] = 上次的最低位數[0000 0001])
                        bytes[index - 1] = (byte)(bytes[index - 1] | LSB_BYTE);
                    }
                }
                else
                {
                    //將第一個陣列元素檢查最高有效位數的結果先暫存起來,並輸出
                    leftMostCarryFlag = carryFlag;
                }
                //將當前的陣列元素向左移 1 bit
                bytes[index] = (byte)(bytes[index] << 1);
            }
            return leftMostCarryFlag;
        }

        /// <summary>
        /// Rotates shiftCnt times of the bits in an array of bytes to the left.
        /// 類似ShiftLeft方法
        /// 差別是將來源陣列的第一個最高位元加到陣列的右邊(即陣列最後一個元素的最小位數)
        /// ex:
        /// {129,130}=>{ 10000001,10000010}
        /// 左移1bit,會將第1個bit移到最右邊
        /// {00000011,00000101} => {3,5}
        /// </summary>
        /// <param name="srcBytes">The byte array to shift</param>
        /// <param name="shiftCnt">number of times to shift left</param>
        public byte[] RotateLeft(byte[] srcBytes, int shiftCnt)
        {
            byte[] bytes = new byte[srcBytes.Length];
            Array.Copy(srcBytes, bytes, srcBytes.Length);
            //
            for (int i = 0; i < shiftCnt; i++)
            {
                bool carryFlag = this.shiftLeft(ref bytes);
                if (carryFlag)
                {
                    bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] | LSB_BYTE);
                }
            }
            return bytes;
        }

        public byte[] ShiftRight(byte[] srcBytes, int shiftCnt)
        {
            byte[] bytes = new byte[srcBytes.Length];
            Array.Copy(srcBytes, bytes, srcBytes.Length);
            //
            for (int i = 0; i < shiftCnt; i++)
            {
                shiftRight(ref bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Shifts the bits in an array of bytes to the right.
        /// 陣列元素向右移1 bit, 最小有效位數會丟棄
        /// ex:{ 129, 129 } => { 10000001, 10000001 }
        /// 右移 1 bit,會將最後一個陣列元素的最小有效位數丟掉,最左邊補0
        /// {01000000,11000000} => { 64, 192 }
        /// </summary>
        /// <param name="bytes">要右移的陣列來源</param>
        /// <returns>陣列最後一個元素的最小有效位數 true(1)/false(0)</returns>
        private bool shiftRight(ref byte[] bytes)
        {
            bool rightMostFlag = false;
            int rightEnd = bytes.Length - 1;

            //Iterate through the elements of the array right to left.
            for (int index = rightEnd; index >= 0; index--)
            {
                //用來紀錄最後一個陣列元素是否存在最小有效位元[0000 0001]
                bool carryFlag = (bytes[index] | LSB_BYTE) > 0;
                
                if (index < rightEnd)
                {
                    //若存在最小有效位數,則將最小有效位數轉成最大有效位數,再接到陣列後面一個陣列元素的頭[0000 0001] => [1xxx xxxx]
                    if (carryFlag)
                    {
                        //將前一次的陣列元素的頭加上要右移1bit的有效位數 
                        //ex:{00000101,10011001} ==右移1bit==> {00000010,11001100}
                        bytes[index + 1] = (byte)(bytes[index + 1] | MSB_BYTE);
                    }
                }
                else
                {
                    //陣列最後一個元素是否存在最小有效位數[0000 0001]
                    rightMostFlag = carryFlag;
                }
                bytes[index] = (byte)(bytes[index] >> 1);
            }
            return rightMostFlag;

        }

        /// <summary>
        /// Shifts the bits in an array of bytes to the right.
        /// 陣列元素向右移1 bit, 最小有效位數會轉成最大有效位數再塞回陣列的第一個元素
        /// ex:{ 129, 129 } => { 10000001, 10000001 }
        /// 右移 1 bit,會將最後一個陣列元素的最小有效位數會轉成最大有效位數再塞回陣列的第一個元素
        /// {11000000,11000000} => { 192, 192 }
        /// </summary>
        /// <param name="srcBytes">要右移的陣列來源</param>
        /// <param name="shiftCnt">右移n bit</param>
        /// <returns>右移後的陣列</returns>
        public byte[] RotateRight(byte[] srcBytes, int shiftCnt)
        {
            byte[] bytes = new byte[srcBytes.Length];
            Array.Copy(srcBytes, bytes, srcBytes.Length);
            //
            for (int i = 0; i < shiftCnt; i++)
            {
                //陣列最後一個元素是否存在最小位數
                bool carrayFlag = this.shiftRight(ref bytes);
                if (carrayFlag)
                {
                    bytes[0] = (byte)(bytes[0] | MSB_BYTE);
                }
            }
            return bytes;
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
                throw new Exception("陣列雙方長度不符,無法運算");
            }
            byte[] result = new byte[op1.Length];
            for (int i = 0; i < op1.Length; i++)
            {
                //將兩個陣列作XOR後,資料結果存到新陣列
                result[i] = (byte)(op1[i] ^ op2[i]);
            }
            return result;
        }

        /// <summary>
        /// The block length of AES-128 is 128 bits (16 octets). 
        /// If the length of the message is not a positive
        /// multiple of the block length then Pad M with
        /// the bit-string 10^i to adjust the length of the last block up to the
        /// block length.
        /// function, padding(x), is defined as follows:
        /// -   r = x.Length 
        /// -   padding(x) = x || 10^i      where i is 128- 8*r - 1
        /// That is, padding(x) is the concatenation of x and a single '1',
        /// followed by the minimum number of '0's, so that the total length is
        /// equal to 128 bits.
        /// </summary>
        public byte[] CMacPadding(byte[] srcBytes)
        {
            int blockCnt = srcBytes.Length / 16;
            int lastBytes = srcBytes.Length % 16;
            byte[] padded = null;
            //若非一個完整的Block
            if (lastBytes != 0)
            {
                blockCnt += 1;//增一個block作Padding用
                padded = new byte[blockCnt * 16];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
                padded[srcBytes.Length] = 0x80;//設定最後一個陣列Padding的資料(這是規格)
                for (int i = srcBytes.Length + 1; i < padded.Length; i++)
                {
                    padded[i] = 0;
                }
            }
            else
            {
                //陣列可以被完整切割,無剩餘的
                padded = new byte[srcBytes.Length];
                Array.Copy(srcBytes, 0, padded, 0, srcBytes.Length);
            }
            return padded;
        }
    }
}
