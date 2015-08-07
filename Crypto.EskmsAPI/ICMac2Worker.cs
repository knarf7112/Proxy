using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// The CMAC Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
    /// </summary>
    public interface ICMac2Worker
    {
        /// <summary>
        /// Set IV for MAC
        /// </summary>
        /// <param name="iv">initialization vector</param>
        void SetIv(byte[] iv);

        /// <summary>
        /// CMAC Loading
        /// </summary>
        /// <param name="m">input data</param>
        void DataInput(byte[] m);

        /// <summary>
        /// Set CMAC key label
        /// </summary>
        /// <param name="keyLabel">mac key label</param>
        void SetMacKey(string keyLabel);

        /// <summary>
        /// set size of MAC
        /// </summary>
        /// <param name="macLength">MAC Size</param>
        void SetMacLength(int macLength);

        /// <summary>
        /// Get bytes by MacLength start from MSB
        /// </summary>
        /// <returns>MAC</returns>
        byte[] GetMac();

        /// <summary>
        /// Every odd bytes (start from 0, {1,3,5,7,9,11,13,15}) from 16 bytes standard CMAC
        /// </summary>
        /// <returns>MAC</returns>
        byte[] GetOdd();
    }
}
