using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// 向KMS取DiversKey的
    /// </summary>
    public interface IKMSGetter
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
        #endregion


        /// <summary>
        /// 取得Divers Key,異常則回傳null
        /// </summary>
        /// <returns>Divers Key(16 byte)</returns>
        byte[] GetDiversKey();
    }
}
