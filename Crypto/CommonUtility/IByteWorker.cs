using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public interface IByteWorker
    {
        /// <summary>
        /// rotate src byte array left bytewise with cnt times
        /// </summary>
        /// <param name="src">src byte array</param>
        /// <param name="cnt">times to rotate</param>
        /// <returns>result bytes</returns>
        byte[] RotateLeft(byte[] src, int cnt);

        /// <summary>
        /// rotate src byte array right bytewise with cnt times
        /// </summary>
        /// <param name="src">src byte array</param>
        /// <param name="cnt">times to rotate</param>
        /// <returns>result bytes</returns>
        byte[] RotateRight(byte[] src, int cnt);

        /// <summary>
        ///  Combine two arrays into one 
        /// </summary>
        /// <param name="first">first array</param>
        /// <param name="second">second array</param>
        /// <returns>result array</returns>
        byte[] Combine(byte[] first, byte[] second);

        /// <summary>
        ///  Combine several arrays into one
        /// </summary>
        /// <param name="manyByteArr">list of byte array</param>
        /// <returns>result array</returns>
        byte[] Combine(params byte[][] manyByteArr);

        /// <summary>
        ///  Get sub array for src array
        /// </summary>
        /// <param name="src">src array</param>
        /// <param name="beginIndex">begin to copy</param>
        /// <param name="Length">lenght of bytes to copy</param>
        /// <returns>result array</returns>
        byte[] SubArray(byte[] src, int beginIndex, int Length);

        /// <summary>
        ///   op1 XOR op2  with bits of the byte array 
        /// </summary>
        /// <param name="op1">byte array1</param>
        /// <param name="op2">byte array2</param>
        /// <returns>Result of XOR</returns>
        byte[] ExclusiveOr(byte[] op1, byte[] op2);

        /// <summary>
        /// if srcBytes.Length %16 != 0, padding to 10^j, where j = ( 16 - ( srcBytes.Length % 16 ) ) * 8 - 1
        /// </summary>
        /// <param name="srcBytes">src bytes</param>
        /// <returns>padded bytes</returns>
        byte[] CMacPadding(byte[] srcBytes);

        /// <summary>
        /// if srcBytes.Length % blockSize != 0, padding to 0^j, where j = ( blockSize - ( srcBytes.Length % blockSize ) ) * 8
        /// </summary>
        /// <param name="srcBytes">src bytes</param>
        /// <param name="blockSize">blockSize</param>
        /// <returns>padded bytes</returns>
        byte[] ZeroPadding(byte[] srcBytes, int blockSize);

        /// <summary>
        /// check if each byte of op1 and op2 with the same value
        /// </summary>
        /// <param name="op1">byte array1</param>
        /// <param name="op2">byte array2</param>
        /// <returns>true: equal</returns>
        bool AreEqual(byte[] op1, byte[] op2);

        /// <summary>
        /// Reverse byte array 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        byte[] Reverse(byte[] src);

        /// <summary>
        /// Fill src byte array with pad 
        /// </summary>
        /// <param name="src">src bytes</param>
        /// <param name="pad">pad byte</param>
        void Fill(ref byte[] src, byte pad);

        /// <summary>
        /// New byte array then fill with pad
        /// </summary>
        /// <param name="size">size of byte array</param>
        /// <param name="pad">pad bytes</param>
        /// <returns>new padded array</returns>
        byte[] Fill(int size, byte pad);
    }
}
