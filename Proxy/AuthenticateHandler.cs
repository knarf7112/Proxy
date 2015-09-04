using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Web;
using System.IO;
using Common.Logging;
using System.Diagnostics;
using Crypto.POCO;
using Newtonsoft.Json;

namespace Proxy
{
    /// <summary>
    /// 3 Pass Authentication
    /// </summary>
    public class AuthenticateHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthenticateHandler));
        /// <summary>
        /// 認證資料長度
        /// </summary>
        private static readonly int AuthenticateLength = 50;

        /// <summary>
        /// 從設定檔讀取的連向後台Service的服務名稱
        /// </summary>
        private static readonly string ServiceName = "AuthenticateService";

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
            string inputData = string.Empty;
            byte[] responseData = null;
            Stopwatch timer = new Stopwatch();//耗時計算
            timer.Start();
            #region Request資料轉ASCII字串
            log.Debug("[UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent);
            inputData = GetStringFromInputStream(context);
            log.Debug("[Authenticate Request] Data(length:" + inputData.Length + "):" + inputData);
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            //檢查request資料是否符合規定長度
            if (!String.IsNullOrEmpty(inputData) && inputData.Length == AuthenticateLength)
            {
                responseData = DoAuthenticate(inputData);
                //context.Response.OutputStream.Position = 0;
                if (responseData != null)
                {
                    log.Debug("[Authenticate Response] Data(length:" + responseData.Length + "):" + BitConverter.ToString(responseData).Replace("-", ""));
                    context.Response.OutputStream.Write(responseData, 0, responseData.Length);//return 68 bytes
                }
                else
                {
                    //Authenticate fail
                    log.Debug("Authenticate Failed");
                    context.Response.OutputStream.Write(Encoding.ASCII.GetBytes("Auth Failed"), 0, 11);
                }
                log.Debug("[Authenticate]Response Write Finished");
            }
            else
            {
                log.Debug("[Authenticate]Request Error");
                context.Response.OutputStream.Write(Encoding.ASCII.GetBytes("Request Error"), 0, 13);//.Write("Test");
            }
            #endregion

            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            //context.Response.End();
            timer.Stop();
            log.Debug("[Authenticate] End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency).ToString("f3") + "s)");
            context.ApplicationInstance.CompleteRequest();
            
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
            byte[] requestBytes = new byte[25];
            byte[] result = null;
            requestBytes[0] = byte.Parse(inputData.Substring(0, 2));            //keyLabel:0~1     => "32" => byte{0x20}
            requestBytes[1] = byte.Parse(inputData.Substring(2, 2));            //KeyVersion:2~3   => "00" => byte{0x00}
            byte[] uid = StringToByteArray(inputData.Substring(4, 14));         //uid:4~17  => "04873ABA8D2C80" => byte{0x04,0x87,0x3A,0xBA,0x8D,0x2C,0x80}
            byte[] enc_RanB = StringToByteArray(inputData.Substring(18, 32));   //enc_RanB:18~50 => "4EF61041ABE8B0EF8B32A627B19D83AA" => byte{0x4E,0xF6,0x10,0x41,0xAB,0xE8,0xB0,0xEF,0x8B,0x32,0xA6,0x27,0xB1,0x9D,0x83,0xAA}
            Buffer.BlockCopy(uid, 0, requestBytes, 2, uid.Length);
            Buffer.BlockCopy(enc_RanB, 0, requestBytes, (2 + uid.Length), enc_RanB.Length);
            result = DoAuthenticate(requestBytes);
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
            byte[] result = null;
            if (inputData.Length != 25)
                return result;                        //ha ha
            string keyLabel = "2ICH3F0000" + inputData[0].ToString() + "A";                 //byte[0]
            string KeyVersion = inputData[1].ToString();                                    //byte[1]
            string uid = BitConverter.ToString(inputData, 2, 7).Replace("-", "");           //byte[2~8]
            string enc_RanB = BitConverter.ToString(inputData, 9, 16).Replace("-", "");     //byte[9~24]
            EskmsPOCO response = null;
            EskmsPOCO request = new EskmsPOCO() 
            {
                Input_KeyLabel = keyLabel,
                Input_KeyVersion = KeyVersion,
                Input_UID = uid,
                Input_Enc_RanB = enc_RanB
            };
            #region Authenticate Object Old
            //IiBonAuthenticate iBonAuth = null;
            //try
            //{
            //    iBonAuth = new iBonAuthenticate()
            //    {
            //        Input_KeyLabel = keyLabel,
            //        Input_KeyVersion = KeyVersion,
            //        Input_UID = uid,
            //        Input_Enc_RanB = enc_RanB,
            //    };
            //    //run
            //    iBonAuth.StartAuthenticate(true);
            //}
            //catch (Exception ex)
            //{
            //    log.Error("[iBonAuthenticate] Error:" + ex);
            //    return new byte[5] { 0x45, 0x72, 0x72, 0x6F, 0x72 };//return "Error"
            //}
            #endregion
            response = GetResponse(request);
            if (response != null)
            {
                result = new byte[response.Output_RanB.Length +
                                     response.Output_Enc_RanAandRanBRol8.Length +
                                     response.Output_Enc_IVandRanARol8.Length + 4];
                byte[] randAStartIndexBytes = BitConverter.GetBytes(response.Output_RandAStartIndex);//4 bytes
                Buffer.BlockCopy(response.Output_RanB, 0, result, 0, response.Output_RanB.Length);//Copy Random B in Result
                Buffer.BlockCopy(response.Output_Enc_RanAandRanBRol8, 0, result, response.Output_RanB.Length, response.Output_Enc_RanAandRanBRol8.Length);//Copy E(RanA || RanBRol8) in Result
                Buffer.BlockCopy(response.Output_Enc_IVandRanARol8, 0, result, response.Output_RanB.Length + response.Output_Enc_RanAandRanBRol8.Length, response.Output_Enc_IVandRanARol8.Length);//Copy E(iv,RanARol8) in Result
                Buffer.BlockCopy(randAStartIndexBytes, 0, result, response.Output_RanB.Length + response.Output_Enc_RanAandRanBRol8.Length + response.Output_Enc_IVandRanARol8.Length, randAStartIndexBytes.Length);//Copy Random A Start Index (4 Bytes)
                log.Debug("RanB:" + BitConverter.ToString(response.Output_RanB).Replace("-", ""));
                log.Debug("E(RanA || RanBRol8):" + BitConverter.ToString(response.Output_Enc_RanAandRanBRol8).Replace("-", ""));
                log.Debug("E(iv,RanARol8):" + BitConverter.ToString(response.Output_Enc_IVandRanARol8).Replace("-", ""));
                log.Debug("Random A Start Index:" + response.Output_RandAStartIndex);
                log.Debug("Session Key:" + BitConverter.ToString(response.Output_SessionKey).Replace("-", ""));
            }
            
            return result;
        }

        /// <summary>
        /// 連線後端AP並取得output data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static EskmsPOCO GetResponse(EskmsPOCO request)
        {
            string requestStr = null;
            byte[] requestBytes = null;
            string responseStr = null;
            byte[] responseBytes = null;
            string ip = null;
            int port = -1;
            int sendTimeout = -1;
            int receiveTimeout = -1;
            string serverConfig = null;
            string[] configs = null;
            EskmsPOCO response = null;
            //*********************************
            //取得連線後台的WebConfig設定資料
            serverConfig = ConfigGetter.GetValue(ServiceName);
            log.Debug(m => { m.Invoke(ServiceName + ":" + serverConfig); });
            if (serverConfig != null)
            {
                configs = serverConfig.Split(':');
                ip = configs[0];
                port = Convert.ToInt32(configs[1]);
                sendTimeout = Convert.ToInt32(configs[2]);
                receiveTimeout = Convert.ToInt32(configs[3]);
            }
            else
            {
                log.Error("要連結的目的地設定資料不存在:" + ServiceName);
                return null;
            }
            //*********************************
            try
            {
                using (SocketClient.Domain.SocketClient connectToAP = new SocketClient.Domain.SocketClient(ip, port, sendTimeout, receiveTimeout))
                {
                    log.Debug("開始連線後端服務:" + serverConfig);
                    if (connectToAP.ConnectToServer())
                    {
                        //UTF8(JSON(POCO))=>byte array and send to AP
                        requestStr = JsonConvert.SerializeObject(request);
                        log.Debug(m => m("[Authenticate]Request JsonString({0}): {1}", ServiceName, requestStr));
                        requestBytes = Encoding.UTF8.GetBytes(requestStr);//Center AP used UTF8
                        responseBytes = connectToAP.SendAndReceive(requestBytes);
                        if (responseBytes != null)
                        {
                            responseStr = Encoding.UTF8.GetString(responseBytes);
                            response = JsonConvert.DeserializeObject<EskmsPOCO>(responseStr);
                            //Byte[] 會被JSON轉成Base64格式
                            log.Debug(m => {
                                m.Invoke("[Authenticate]Response JsonString:\n RanB:{0}, E(RanA,RanBRol8):{1}, E(IV,RanARol8):{2}, RandAIndex:{3}, SessionKey:{4}", 
                                    BitConverter.ToString(response.Output_RanB).Replace("-", ""),
                                    BitConverter.ToString(response.Output_Enc_RanAandRanBRol8).Replace("-", ""),
                                    BitConverter.ToString(response.Output_Enc_IVandRanARol8).Replace("-", ""),
                                    response.Output_RandAStartIndex.ToString(),
                                    BitConverter.ToString(response.Output_SessionKey).Replace("-", "")); });
                        }
                        else
                        {
                            //Byte[] 會被JSON轉成Base64格式
                            log.Debug(m => { m.Invoke("[Authenticate]Response JsonString: null"); });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("後台連線異常:" + ex.Message);
            }
            return response;
        }
        #endregion
    }
}
