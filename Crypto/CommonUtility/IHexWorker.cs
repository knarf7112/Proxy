
namespace Crypto.CommonUtility
{
    /// <summary>
    ///   Utility for pack and unpack a single byte
    /// </summary>
    public interface IHexWorker
    {
        /// <summary>
        ///   unpack 1 byte to 2 hex
        /// </summary>
        /// <param name="b">byte</param>
        /// <returns>hex</returns>
        string Byte2Hex(byte b);

        /// <summary>
        ///    pack 2 hex to 1 byte
        /// </summary>
        /// <param name="hexStr">hex string with 2 char</param>
        /// <returns>byte</returns>
        byte Hex2Byte(string hexStr);
    }
}
