using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.POCO
{
    /// <summary>
    /// Authenticate POCO
    /// </summary>
    [Serializable]
    public class EskmsPOCO
    {
        /// <summary>
        /// KMS的keyLabel
        /// ex:"2ICH3F000032A"
        /// </summary>
        [LengthCkeck(FixLength=13)]
        public string Input_KeyLabel { get; set; }
        /// <summary>
        /// ex:"00"
        /// </summary>
        [LengthCkeck(FixLength=1)]
        public string Input_KeyVersion { get; set; }
        /// <summary>
        /// ex:"04873ABA8D2C80"
        /// </summary>
        [LengthCkeck(FixLength=14)]
        public string Input_UID { get; set; }
        /// <summary>
        /// ex:"4EF61041ABE8B0EF8B32A627B19D83AA"
        /// </summary>
        [LengthCkeck(FixLength = 32)]
        public string Input_Enc_RanB { get; set; }

        /// <summary>
        /// Random B (16 bytes)
        /// </summary>
        public byte[] Output_RanB { get; set; }
        /// <summary>
        /// Enc(iv,RanA + RandBRol8)  (32 bytes)
        /// </summary>
        public byte[] Output_Enc_RanAandRanBRol8 { get; set; }
        /// <summary>
        /// Enc(IV,RanARol8) (16 bytes)
        /// </summary>
        public byte[] Output_Enc_IVandRanARol8 { get; set; }
        /// <summary>
        /// SessionKey
        /// </summary>
        public byte[] Output_SessionKey { get; set; }
        /// <summary>
        /// Random A Start Index
        /// </summary>
        public int Output_RandAStartIndex { get; set; }

    }
}
