using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.EskmsAPI
{
    public interface IiBonAuthenticate
    {
        #region Input Data
        /// <summary>
        /// [Input]:ex:2ICH3F000032A
        /// </summary>
        string Input_KeyLabel { set; }

        /// <summary>
        /// [Input]:0x00
        /// </summary>
        string Input_KeyVersion { set; }

        /// <summary>
        /// [Input]:卡片UID(7 bytes) 
        /// </summary>
        string Input_UID { set; }

        /// <summary>
        /// UID組合完畢的資料:32 bytes(和卡片UID屬性二擇一輸入)
        /// byte[] { 01 + uid + ICASH + uid + ICASH + uid }
        /// </summary>
        byte[] Input_BlobValue { set; }

        /// <summary>
        /// [Input]:E(RanB) => 16 bytes
        /// </summary>
        string Input_Enc_RanB { set; }

        /// <summary>
        /// 強制輸入的RanA
        /// 不輸入則從Dll產生隨機RanA值
        /// </summary>
        byte[] Input_RanA { set; }
        #endregion

        #region Output Data
        /// <summary>
        /// [Output]:Random B
        /// </summary>
        byte[] Output_RanB { get; }

        /// <summary>
        /// iv = E(RanB)
        /// [Output]:E( iv, (RanA || RanBRol8))
        /// </summary>
        byte[] Output_Enc_RanAandRanBRol8 { get; }

        /// <summary>
        /// iv = (E( iv, (RanA || RanBRol8))).SubArray(last 16 bytes)
        /// [Output]:E( iv, RanARol8)
        /// </summary>
        byte[] Output_Enc_IVandRanARol8 { get; }

        /// <summary>
        /// [Output]:Random A start index
        /// </summary>
        int Output_RandAStartIndex { get; }

        /// <summary>
        /// [Output]:Session Key(16 bytes) = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        byte[] Output_SessionKey { get; }
        #endregion

        /// <summary>
        /// 開始執行認證
        /// <param name="isGenSessionKey">是否產生SessionKey屬性</param>
        /// </summary>
        void StartAuthenticate(bool isGenSessionKey);

        /// <summary>
        /// 取得Session Key
        /// SessionKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        byte[] Generate_SessionKey();
    }
}
