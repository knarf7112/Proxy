using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.POCO
{
    /// <summary>
    /// Divers Key POCO
    /// </summary>
    [Serializable]
    public class EskmsKeyPOCO
    {
        /// <summary>
        /// KMS的keyLabel
        /// ex:"2ICH3F000032A"
        /// </summary>
        [LengthCkeck(FixLength = 13)]
        public string Input_KeyLabel { get; set; }
        /// <summary>
        /// KMS的keyVersion
        /// ex:"00"
        /// </summary>
        [LengthCkeck(FixLength = 1)]
        public string Input_KeyVersion { get; set; }
        /// <summary>
        /// SAM ID
        /// ex:"04873ABA8D2C80"
        /// </summary>
        [LengthCkeck(FixLength = 14)]
        public string Input_UID { get; set; }
        /// <summary>
        /// 卡機裝置ID
        /// ex:"VF061041ABE8B0EF8B32A627B19D83AA"
        /// </summary>
        [LengthCkeck(FixLength = 32)]
        public string Input_DeviceID { get; set; }

        /// <summary>
        /// Divers Key (16 bytes)
        /// </summary>
        public byte[] Output_DiversKey { get; set; }

    }
}
