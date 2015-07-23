using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public interface IBytesBitwiser
    {
        /// <summary>
        /// The left-most bit of a bit string
        /// 陣列第一個元素是否存在最高有效位數[1xxx xxxx]
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <returns>true:the left-most bit equals 1</returns>
        bool MsbOne(byte[] bytes);

        /// <summary>
        /// Shifts shiftCnt times of the bits in an array of bytes to the left .
        /// 將資料陣列向左位移n個位數
        /// </summary>
        /// <param name="srcBytes">The byte array to shift</param>
        /// <param name="shiftCnt">number of times to shift left</param>
        /// <returns></returns>
        byte[] ShiftLeft(byte[] srcBytes, int shiftCnt);

        /// <summary>
        /// Rotates shiftCnt times of the bits in an array of bytes to the left.
        /// 將資料陣列向左旋轉n個位數
        /// </summary>
        /// <param name="srcBytes"></param>
        /// <param name="shiftCnt"></param>
        /// <returns></returns>
        byte[] RotateLeft(byte[] srcBytes, int shiftCnt);

        /// <summary>
        ///  Shifts shiftCnt times of the bits in an array of bytes to the right.
        /// </summary>
        /// <param name="srcBytes">The byte array to shift</param>
        /// <param name="shiftCnt">number of times to shift right</param>
        byte[] ShiftRight(byte[] srcBytes, int shiftCnt);

        /// <summary>
        /// Rotates shiftCnt times of the bits in an array of bytes to the right.
        /// </summary>
        /// <param name="srcBytes">The byte array to shift</param>
        /// <param name="shiftCnt">number of times to shift right</param>
        byte[] RotateRight(byte[] srcBytes, int shiftCnt);

        /// <summary>
        ///   op1 XOR op2 with bits of the byte array 
        /// </summary>
        /// <param name="op1">byte array1</param>
        /// <param name="op2">byte array2</param>
        /// <returns>Result of XOR</returns>
        byte[] ExclusiveOr(byte[] op1, byte[] op2);

        /// <summary>
        ///  padding to 10^j , where j = (16 - MLen % 16) * 8 - 1
        /// </summary>
        /// <param name="srcBytes"></param>
        /// <returns></returns>
        byte[] CMacPadding(byte[] srcBytes);
    }
}
