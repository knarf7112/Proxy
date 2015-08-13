using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Web;
using Crypto.EskmsAPI;
using System.IO;
using Common.Logging;

namespace Proxy
{
    /// <summary>
    /// 3 Pass Authentication
    /// </summary>
    public class AuthenticateHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthenticateHandler));

        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// Request in
        /// </summary>
        /// <param name="context">Http請求</param>
        public void ProcessRequest(HttpContext context)
        {
            #region Request資料轉Byte Array
            //log.Debug("UserIP:" + context.Request.UserHostAddress);
            //byte[] requestData = GetBytesFromInputStream(context);
            //context.Response.ContentType = "text/plain";
            //context.Response.StatusCode = 200;
            //if (requestData.Length != 24)
            //{
            //    log.Debug("Request Data:" + BitConverter.ToString(requestData).Replace("-",""));
            //    byte[] responseData = DoAuthenticate(requestData);
            //    log.Debug("Response Data:" + BitConverter.ToString(responseData).Replace("-",""));
            //    //context.Response.OutputStream.Position = 0;
            //    context.Response.OutputStream.Write(responseData, 0, responseData.Length);
            //}
            //else
            //{
            //    log.Debug("Test123");
            //    context.Response.OutputStream.Write(Encoding.ASCII.GetBytes("Test123"), 0, 7);//.Write("Test");

            //}
            #endregion

            #region Request資料轉ASCII字串
            log.Debug("[UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent);
            string inputData = GetStringFromInputStream(context);
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            if (!String.IsNullOrEmpty(inputData))
            {
                log.Debug("[Request] Data:" + inputData);
                byte[] responseData = DoAuthenticate(inputData);
                //context.Response.OutputStream.Position = 0;
                log.Debug("[Response] Data:" + BitConverter.ToString(responseData).Replace("-", ""));
                context.Response.OutputStream.Write(responseData, 0, responseData.Length);//return 68 bytes
                log.Debug("End Response");
            }
            else
            {
                log.Debug("Test123");
                context.Response.OutputStream.Write(Encoding.ASCII.GetBytes("Test123"), 0, 7);//.Write("Test");
            }
            #endregion

            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            //context.Response.End();
            context.ApplicationInstance.CompleteRequest();
            log.Debug("End Response");
        }

        #region Request轉ASCII字串
        /// <summary>
        /// get string from InputStream and Encoding default is ASCII
        /// </summary>
        /// <param name="context">request context</param>
        /// <returns>request http body</returns>
        private static string GetStringFromInputStream(HttpContext context)
        {
            StreamReader sr = new StreamReader(context.Request.InputStream, Encoding.ASCII);
            string result = sr.ReadToEnd();
            return result;
        }
        /// <summary>
        /// Run 3 Pass Authenticate Flow by ASCII string(length:50)
        /// </summary>
        /// <param name="inputData">challenge Data and parameters</param>
        /// <returns>RanB||E(RanA||RanBRol8)||E(iv,RanARol8)||RanAStartIndex</returns>
        private static byte[] DoAuthenticate(string inputData)
        {
            if (inputData.Length != 50)
                return new byte[5] { 0x48, 0x61, 0x20, 0x48, 0x61 };//ha ha
            byte[] requestBytes = new byte[25];
            requestBytes[0] = byte.Parse(inputData.Substring(0, 2));            //keyLabel:0~1     => "32" => byte{0x20}
            requestBytes[1] = byte.Parse(inputData.Substring(2, 2));            //KeyVersion:2~3   => "00" => byte{0x00}
            byte[] uid = StringToByteArray(inputData.Substring(4, 14));         //uid:4~17  => "04873ABA8D2C80" => byte{0x04,0x87,0x3A,0xBA,0x8D,0x2C,0x80}
            byte[] enc_RanB = StringToByteArray(inputData.Substring(18, 32));   //enc_RanB:18~50 => "4EF61041ABE8B0EF8B32A627B19D83AA" => byte{0x4E,0xF6,0x10,0x41,0xAB,0xE8,0xB0,0xEF,0x8B,0x32,0xA6,0x27,0xB1,0x9D,0x83,0xAA}
            Buffer.BlockCopy(uid, 0, requestBytes, 2, uid.Length);
            Buffer.BlockCopy(enc_RanB, 0, requestBytes, (2 + uid.Length), enc_RanB.Length);
            byte[] result = DoAuthenticate(requestBytes);
            return result;
        }

        /// <summary>
        /// hex string to byte array
        /// </summary>
        /// <param name="hex">hex data</param>
        /// <returns></returns>
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        #endregion

        #region Request轉Byte Array
        private static byte[] GetBytesFromInputStream(HttpContext context)
        {
            byte[] buffer = new byte[0x1000];
            int readLength = context.Request.InputStream.Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, readLength);
            return buffer;
        }
        /// <summary>
        /// Run 3 Pass Authenticate Flow by Byte Array(length:25)
        /// </summary>
        /// <param name="inputData">challenge Data and parameters</param>
        /// <returns>RanB||E(RanA||RanBRol8)||E(iv,RanARol8)||RanAStartIndex</returns>
        private static byte[] DoAuthenticate(byte[] inputData)
        {
            if (inputData.Length != 25)
                return new byte[5] { 0x48, 0x61, 0x20, 0x48, 0x61 };                        //ha ha
            string keyLabel = "2ICH3F0000" + inputData[0].ToString() + "A";                 //byte[0]
            string KeyVersion = inputData[1].ToString();                                    //byte[1]
            string uid = BitConverter.ToString(inputData, 2, 7).Replace("-", "");            //byte[2~8]
            string enc_RanB = BitConverter.ToString(inputData, 9, 16).Replace("-", "");     //byte[9~24]
            IiBonAuthenticate iBonAuth = null;
            try
            {
                iBonAuth = new iBonAuthenticate()
                {
                    Input_KeyLabel = keyLabel,
                    Input_KeyVersion = KeyVersion,
                    Input_UID = uid,
                    Input_Enc_RanB = enc_RanB,
                };
                //run
                iBonAuth.StartAuthenticate(true);
            }
            catch (Exception ex)
            {
                log.Error("[iBonAuthenticate] Error:" + ex);
                return new byte[5] { 0x45, 0x72, 0x72, 0x6F, 0x72 };//return "Error"
            }
            byte[] result = new byte[iBonAuth.Output_RanB.Length +
                                     iBonAuth.Output_Enc_RanAandRanBRol8.Length +
                                     iBonAuth.Output_Enc_IVandRanARol8.Length + 4];
            byte[] randAStartIndexBytes = BitConverter.GetBytes(iBonAuth.Output_RandAStartIndex);//4 bytes
            Buffer.BlockCopy(iBonAuth.Output_RanB, 0, result, 0, iBonAuth.Output_RanB.Length);//Copy Random B in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_RanAandRanBRol8, 0, result, iBonAuth.Output_RanB.Length, iBonAuth.Output_Enc_RanAandRanBRol8.Length);//Copy E(RanA || RanBRol8) in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_IVandRanARol8, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length, iBonAuth.Output_Enc_IVandRanARol8.Length);//Copy E(iv,RanARol8) in Result
            Buffer.BlockCopy(randAStartIndexBytes, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length + iBonAuth.Output_Enc_IVandRanARol8.Length, randAStartIndexBytes.Length);//Copy Random A Start Index (4 Bytes)
            log.Debug("RanB:" + BitConverter.ToString(iBonAuth.Output_RanB).Replace("-", ""));
            log.Debug("E(RanA || RanBRol8):" + BitConverter.ToString(iBonAuth.Output_Enc_RanAandRanBRol8).Replace("-", ""));
            log.Debug("E(iv,RanARol8):" + BitConverter.ToString(iBonAuth.Output_Enc_IVandRanARol8).Replace("-", ""));
            log.Debug("Random A Start Index:" + iBonAuth.Output_RandAStartIndex);
            log.Debug("Session Key:" + BitConverter.ToString(iBonAuth.Output_SessionKey).Replace("-", ""));
            return result;
        }
        #endregion
    }
}
