using System;
//
using System.Security.Cryptography;
using System.IO;

namespace Crypto
{
    /// <summary>
    /// Symmectric cryptor
    /// ref:https://msdn.microsoft.com/zh-tw/library/system.security.cryptography.symmetricalgorithm(v=vs.110).aspx
    /// MSDN說:從 SymmetricAlgorithm 類別繼承時，您必須覆寫下列成員：CreateDecryptor、CreateEncryptor、GenerateIV 和 GenerateKey。
    /// </summary>
    public interface ISymCryptor
    {
        /// <summary>
        /// set key for encrypt/decrypt
        /// </summary>
        /// <param name="key">key for symmectric</param>
        void SetKey(byte[] key);

        /// <summary>
        /// set initial vector
        /// </summary>
        /// <param name="iv">initial vector</param>
        void SetIV(byte[] iv);

        /// <summary>
        /// set algorithm
        /// </summary>
        /// <param name="alg">algorithm for the cryptor</param>
        void SetAlgorithm(string alg);

        /// <summary>
        /// set parameter for the cryptor
        /// </summary>
        /// <param name="alg">algorithm for the cryptor</param>
        /// <param name="cipherMode">Cipher mode(default:CBC)</param>
        /// <param name="paddingMode">Padding mode(default:none)</param>
        void SetAlgorithm(string alg, CipherMode cipherMode, PaddingMode paddingMode);

        /// <summary>
        /// Encrypt plain data into encrypted bytes
        /// </summary>
        /// <param name="data">encrypt data</param>
        /// <returns>encrypted bytes</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypt encrypted bytes to plain data
        /// </summary>
        /// <param name="encryptedData">encrypted bytes</param>
        /// <returns>plain data</returns>
        byte[] Decrypt(byte[] encryptedData);

        void Encrypt(Stream decryptedFile, Stream encryptedFile);

        void Decrypt(Stream encryptedFile, Stream decryptedFile);
    }
}
