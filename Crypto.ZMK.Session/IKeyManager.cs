using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.ZMK.Session
{
    public interface IKeyManager
    {
        /// <summary>
        /// 若有輸入DiverseKey則產生加密的DiverseKey與IV索引値(int)
        /// 若無輸入DiverseKey則產生加密的SessionKey與RndA索引値(int)
        /// </summary>
        /// <param name="key">Key:16 bytes </param>
        /// <param name="rndIndex">亂數表索引</param>
        /// <param name="enc_data">加密後的數據:16 bytes</param>
        /// <param name="diverseKey">MasterKey:16 bytes</param>
        void Encrypt_data(byte[] key, out int rndIndex, out byte[] enc_data, byte[] diverseKey = null);

        /// <summary>
        /// 作AES加密資料,設定Key與依據指定索引從隨機表取得陣列並設定iv
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="rndIndex">隨機物件列表的索引</param>
        /// <param name="data">要加密的</param>
        /// <returns>加密後的數據</returns>
        byte[] Encrypt_data(byte[] key, int rndIndex, byte[] data);

        /// <summary>
        /// 依索引値與ZMK_data取得SessionKey
        /// </summary>
        /// <param name="key">ZNK_data</param>
        /// <param name="rndIndex">random list index</param>
        /// <returns>SessionKey:16 bytes</returns>
        byte[] Get_SessionKey(byte[] key, int rndIndex);

        /// <summary>
        /// 依索引取得隨機陣列的物件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        byte[] Get_RanDom_data(int index);

        /// <summary>
        /// 若有輸入亂數表索引値則解密EncDiverseKey
        /// 若無輸入亂數表索引値則還原SessionKey的原始Data(即亂數表的指定索引陣列:16 bytes)
        /// </summary>
        /// <param name="key">SessionKey/ZMK_DATA</param>
        /// <param name="enc_data">EncDiverseKey/SessionKey</param>
        /// <param name="rndIndex">Ran(IV)/0(IV)</param>
        /// <returns>DiverseKey/RnA</returns>
        byte[] Decrypt_data(byte[] key, byte[] enc_data, int rndIndex = -1);

        /// <summary>
        /// 從隨機列表內取得一組隨機索引
        /// </summary>
        /// <returns>random index</returns>
        int Get_RandomIndex();

        /// <summary>
        /// 取得SessionKey的A Part索引値與B Part陣列:16 bytes
        /// </summary>
        /// <param name="sessionKey">針對每間特約機構產生的傳輸加密金鑰</param>
        /// <param name="A_part_Index">A Part索引値</param>
        /// <param name="B_part">B Part陣列:16 bytes</param>
        /// <returns>true:建立AB Part成功/false:異常</returns>
        bool Gen_AB_Part(byte[] sessionKey, out int A_part_Index, out byte[] B_part);

        /// <summary>
        /// 取得SessionKey從輸入的A Part索引値與B Part陣列:16 bytes
        /// </summary>
        /// <param name="A_part_Index">A Part索引値</param>
        /// <param name="B_part">B Part陣列:16 bytes</param>
        /// <param name="sessionKey">SessionKey:16 bytes</param>
        /// <returns>true:取得SessionKey成功/false:異常</returns>
        bool Get_SessionKey(int A_part_Index, byte[] B_part, out byte[] sessionKey);
    }
}
