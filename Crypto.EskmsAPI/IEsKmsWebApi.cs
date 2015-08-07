
namespace Crypto.EskmsAPI
{
    /// <summary>
    /// 介接C++寫的DLL(eskmsapi.dll)
    /// 使用DLL來作加解密
    /// </summary>
    public interface IEsKmsWebApi
    {
        string CipherMode { set; }
        /// <summary>
        /// Encrypt by key with the specific keylabel
        /// </summary>
        /// <param name="keyLabel">keyLabel map to key in kms</param>
        /// <param name="iv">iv for encrypting</param>
        /// <param name="decrypted">data to encrypt</param>
        /// <returns>encrypted data</returns>
        byte[] Encrypt(string keyLabel, byte[] iv, byte[] decrypted);

        /// <summary>
        /// decrypt by key with the specific keylabel
        /// </summary>
        /// <param name="keyLabel">keyLabel map to key in kms</param>
        /// <param name="iv">iv for decrypting</param>
        /// <param name="encrypted">data to decrypt</param>
        /// <returns>decrypted data</returns>
        byte[] Decrypt(string keyLabel, byte[] iv, byte[] encrypted);
    }
}
