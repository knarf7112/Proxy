
namespace Crypto.EskmsAPI
{
    /// <summary>
    /// 使用EsKmsApi的Dll來取得 Diverse Key
    /// </summary>
    public interface IEsKmsApi
    {
        void Authentication();

        /// <summary>
        /// appCode and authCode setting
        /// </summary>
        /// <param name="appCode">呼叫哪個應用程式</param>
        /// <param name="authCode">允許授權的代碼</param>
        void Authentication(string appCode, string authCode);

        /// <summary>
        /// send to dll infomation and get result address
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cipher_method"></param>
        /// <param name="mechanism"></param>
        /// <param name="dbin"></param>
        /// <param name="dbout"></param>
        /// <param name="result"></param>
        void Cipher(secret_key_t key,uint cipher_method, mechanism_param_t mechanism, data_blob_t dbin, ref data_blob_t dbout, ref result_t result);

        void release();
    }
}
