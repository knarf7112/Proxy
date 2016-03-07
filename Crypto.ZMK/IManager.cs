﻿
namespace Crypto.ZMK
{
    /// <summary>
    /// ZMK Object
    /// </summary>
    public interface IZMK_Manager
    {

        /// <summary>
        /// use KMS 2.0 Libs encrypt ZMK_Data(Random:16 bytes)
        /// </summary>
        /// <returns>Encrypted ZMK data:16 bytes</returns>
        byte[] GetEncrypt_ZMK_Data();
        /// <summary>
        /// use KMS 2.0 Libs decrypt Encrypt_ZMK_Data(16 bytes)
        /// </summary>
        /// <param name="encryptedData">encrypted ZMK data</param>
        /// <returns>Decrypted ZMK data:16 bytes</returns>
        byte[] GetDecrypt_ZMK_Data(byte[] encryptedData);

        /// <summary>
        /// Get Xor data when input data1 and data2
        /// </summary>
        /// <param name="data1">data1:16 bytes</param>
        /// <param name="data2">data2:16 bytes</param>
        /// <returns>Xor data:16 bytes</returns>
        byte[] Get_XOR_data(byte[] data1, byte[] data2);

        /// <summary>
        /// Generate A part and B part data when input ZMK_Data
        /// </summary>
        /// <param name="zmk_data"></param>
        /// <param name="a_part"></param>
        /// <param name="b_part"></param>
        /// <returns>true:正常產生A B碼單/false:異常</returns>
        bool Get_AB_Part(byte[] zmk_data, out byte[] a_part, out byte[] b_part);
    }
}
