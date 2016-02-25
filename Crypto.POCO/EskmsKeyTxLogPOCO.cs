using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.POCO
{
    [Serializable]
    public class EskmsKeyTxLogPOCO
    {
        /// <summary>
        /// SAM ID
        /// ex:"04873ABA8D2C80"
        /// </summary>
        [LengthCkeck(FixLength = 14)]
        public string SAM_UID { get; set; }

        /// <summary>
        /// 卡機裝置ID
        /// ex:"VF061041ABE8B0EF8B32A627B19D83AA"
        /// </summary>
        [LengthCkeck(FixLength = 32)]
        public string DeviceId { get; set; }
      
        /// <summary>
        /// 測試解鎖是否成功的TxLog(length:288)
        /// 目前只需要看16~23(ReturnCode),之後再補上驗證mac
        /// </summary>
        public string TestTxLog { get; set; }

        /// <summary>
        /// 流程操作結果
        /// (OK:000000/Fail:000001)
        /// </summary>
        public string ReturnCode { get; set; }
    }
    [Serializable]
    public class EskmsKeyTxLogPOCO_v2 : EskmsKeyTxLogPOCO
    {
        /// <summary>
        /// 顧客識別符號 ex:"KRT"
        /// Size:3
        /// </summary>
        public string Merc_Flg { get; set; }
        /// <summary>
        /// 卡機種別 ex: 00:沒卡機、01:NEC R6 卡機、02:MPG/CRF 卡機、03:CASTLES/V5s 卡機 ...
        /// Size:2
        /// </summary>
        public string Reader_Type { get; set; }
    }
}
