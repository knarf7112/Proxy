using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crypto.CommonUtility
{
    public class HexConverter : IHexConverter
    {
        public IHexWorker HexWorker { set; private get; }
        #region Constructor
        public HexConverter():this(new HexWorkerByArr())
        {

        }

        public HexConverter(IHexWorker hexWorker)
        {
            this.HexWorker = hexWorker;
            //log.Debug("Use " + this.hexWorker.GetType().FullName);
        }
        #endregion

        /// <summary>
        /// 字串轉hex字串,使用預設編碼
        /// "IJK" => "494A4B" 
        /// "494A4B"即{ 49, 4A, 4B }
        /// </summary>
        /// <param name="str">字串</param>
        /// <returns>hex字串</returns>
        public string Str2Hex(string str)
        {
            byte[] byteArr = Encoding.Default.GetBytes(str);
            return this.Bytes2Hex(byteArr);
        }

        /// <summary>
        /// hex字串轉字串,使用預設編碼
        /// "494A4B" => "IJK"
        /// "494A4B"即(hex){ 49, 4A, 4B } => byte{73, 74, 75}
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public string Hex2Str(string hexStr)
        {
            byte[] byteArr = this.Hex2Bytes(hexStr);
            return Encoding.Default.GetString(byteArr);
        }

        /// <summary>
        /// hex字串轉Byte Array
        /// ex:"0F1F" => {15, 31}
        /// </summary>
        /// <param name="hexStr">hex字串</param>
        /// <returns>Byte Array</returns>
        public byte[] Hex2Bytes(string hexStr)
        {
            //hex 為 2 bytes
            byte[] bArr = new byte[hexStr.Length / AbsHexWorker.HexPerByte];
            for (int i = 0, p = 0; i < bArr.Length; i++,p+=AbsHexWorker.HexPerByte )
            {
                bArr[i] = this.HexWorker.Hex2Byte(hexStr.Substring(p, AbsHexWorker.HexPerByte));
            }
            return bArr;
        }

        /// <summary>
        /// 陣列先用ASCII編碼轉hex字串再轉Byte Array
        /// </summary>
        /// <param name="hexBytes"></param>
        /// <returns></returns>
        public byte[] Hex2Bytes(byte[] hexBytes)
        {
            string hexStr = Encoding.ASCII.GetString(hexBytes);

            return this.Hex2Bytes(hexStr);
        }

        public string Bytes2Hex(byte[] dataBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in dataBytes)
            {
                sb.Append(this.HexWorker.Byte2Hex(b));
            }
            //string hexString = BitConverter.ToString(dataBytes, 0, dataBytes.Length).Replace('-', ' ');
            return sb.ToString();
        }

        #region Base
        public string Byte2Hex(byte b)
        {
            return this.HexWorker.Byte2Hex(b);
        }

        public byte Hex2Byte(string hexStr)
        {
            return this.HexWorker.Hex2Byte(hexStr);
        }
        #endregion
    }
}
