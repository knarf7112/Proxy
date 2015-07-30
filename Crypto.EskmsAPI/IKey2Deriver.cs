
namespace Crypto
{
    public interface IKey2Deriver
    {
        /// <summary>
        /// input data for key diversification
        /// </summary>
        /// <param name="m">input bytes</param>
        void DiverseInput(byte[] m);

        /// <summary>
        /// Set seed key for diversifying
        /// </summary>
        /// <param name="keyLabel">2ICH0000010A</param>
        void SetSeedKey(string keyLabel);

        /// <summary>
        /// Get Derived Key
        /// 取得衍生KEey
        /// </summary>
        /// <param name="iv">initial vector</param>
        /// <param name="decrypted"></param>
        /// <returns>Derived key bytes</returns>
        byte[] Encrypt(byte[] iv, byte[] decrypted);

        /// <summary>
        /// 衍生KEey解密還原回原始Key
        /// </summary>
        /// <param name="iv">initial vector</param>
        /// <param name="encrypted">衍生KEey</param>
        /// <returns></returns>
        byte[] Decrypt(byte[] iv, byte[] encrypted);
    }
}
