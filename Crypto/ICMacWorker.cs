
namespace Crypto
{
    /// <summary>
    /// The CMAC Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
    /// </summary>
    public interface ICMacWorker
    {
        /// <summary>
        /// CMAC  Loaing
        /// </summary>
        /// <param name="m">input data</param>
        void DataInput(byte[] m);

        /// <summary>
        /// Set CMAC key
        /// </summary>
        /// <param name="key">mac key</param>
        void SetMacKey(byte[] key);

        /// <summary>
        /// set size of MAC
        /// </summary>
        /// <param name="macLength">Mac size</param>
        void SetMacLength(int macLength);

        /// <summary>
        /// Get bytes by MacLength start from MSB
        /// </summary>
        /// <returns>MAC</returns>
        byte[] GetMac();

        /// <summary>
        /// Every odd bytes(start from 0, {1,3,5,7,9,11,13,15}) from 16-byte standard CMAC
        /// </summary>
        /// <returns>MAC</returns>
        byte[] GetOdd();

        /// <summary>
        /// set IV for MAC
        /// 設定初始值
        /// </summary>
        /// <param name="iv">initialization vector</param>
        void SetIV(byte[] iv);
    }
}
