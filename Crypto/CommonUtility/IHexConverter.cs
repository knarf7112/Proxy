
namespace Crypto.CommonUtility
{
    /// <summary>
    ///    Pack and unpack hex string to other Type( uint, string )
    /// </summary>
    public interface IHexConverter
    {
        /// <summary>
        ///    Unpack Default encoding string to Hex string according to default encoding
        /// </summary>
        /// <param name="str">string data to be unpacked(with default encoding)</param>
        /// <returns>hex string</returns>
        string Str2Hex(string str);

        /// <summary>
        ///   Pack hex string to Default encoding string according to default encoding
        /// </summary>
        /// <param name="hexStr">hex string to be packed</param>
        /// <returns>string data(with default encoding)</returns>
        string Hex2Str(string hexStr);

        /// <summary>
        ///   Pack hex string to byte array
        /// </summary>
        /// <param name="hexStr">hex string</param>
        /// <returns>byte array</returns>
        byte[] Hex2Bytes(string hexStr);

        /// <summary>
        ///   Pack hex byte array to byte array
        /// </summary>
        /// <param name="hexBytes">hex byte array</param>
        /// <returns>byte array</returns>
        byte[] Hex2Bytes(byte[] hexBytes);

        /// <summary>
        ///   Unpack byte array to hex string
        /// </summary>
        /// <param name="dataBytes">byte array to be unpacked</param>
        /// <returns>Hex string</returns>
        string Bytes2Hex(byte[] dataBytes);

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
