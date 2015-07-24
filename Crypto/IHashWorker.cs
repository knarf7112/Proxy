//
using System.IO;

namespace Crypto
{
    public interface IHashWorker
    {
        /// <summary>
        /// Compute hash from byte array in memory
        /// </summary>
        /// <param name="decrypted">byte array to be hashed</param>
        /// <returns>hash result</returns>
        byte[] ComputeHash(byte[] decrypted);

        /// <summary>
        /// Compute hash from Stream
        /// 從資料流取出資料來作Hash
        /// </summary>
        /// <param name="stream">stream of bytes</param>
        /// <returns>hash result</returns>
        byte[] ComputeHash(Stream stream);

        string Hash2Hex(byte[] decrypted);

        string Hash2Base64(byte[] decrypted);

        void Initialize();

        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount);
        //byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
        byte[] GetHash();
    }
}
