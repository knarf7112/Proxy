using System;
using System.Text;
//
using System.Web;
using Common.Logging;
using IBON_TRADE_MANAGER_Lib;
using System.Diagnostics;
using System.Collections.Specialized;
using Newtonsoft.Json;
using WebHttpClient;
using System.IO;

namespace Proxy
{
    /// <summary>
    /// 企業自動加值 handler
    /// </summary>
    public class CompanyPrepaidHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompanyPrepaidHandler));
        /// <summary>
        /// 要從web config檔內讀取的資料名稱
        /// </summary>
        private static readonly string ServiceName = "CompanyChargeService";
        /// <summary>
        /// used to lock dicApConfig
        /// </summary>
        private static object lockObj = new object();
        /// <summary>
        /// 此服務的請求電文通訊種別(4 bytes)
        /// </summary>
        private static readonly string Request_Com_Type = "0335";
        /// <summary>
        /// 此服務的回應電文通訊種別(4 bytes)
        /// </summary>
        private static readonly string Response_Com_Type = "0336";
        /// <summary>
        /// 自動加值電文長度
        /// </summary>
        private static readonly int CompanyAutoLoadLength = 142;
        /// <summary>
        /// 通用後台AP錯誤Return Code(6 bytes)
        /// </summary>
        private static readonly string Response_Generic_Error_ReturnCode = "990001";

        /// <summary>
        /// Request in
        /// </summary>
        /// <param name="context">HttpContext</param>
        public void ProcessRequest(HttpContext context)
        {
            string responseString = null;
            byte[] responseBytes = null;
            CLOL_Soc_Req request = null;
            CLOL_Soc_Req response = null;
            Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            log.Debug("[企業加值][UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent);
            // 1. get request dat from input stream by ASCII
            string inputData = GetStringFromInputStream(context, Encoding.ASCII);
            log.Debug("[企業加值 Request] Data(length:" + inputData.Length + "):" + inputData);

            // 2. Parseing request Data to request POCO
            request = ParseRequestString(inputData);

            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            if (request != null)
            {
                // 3. Connect Center AP and Send Request POCO then get response POCO
                response = SendAndReceiveFromAP(request, ServiceName);
                // 4. 若後端AP回應或轉換比對Request和Response有問題時
                if (response == null || !ParseResponseString(request, response, inputData, out responseString))
                {
                    log.Debug(m => m("後端回應:{0}", (response == null).ToString()));
                    //後端無回應(後端異常)
                    responseString = GetResponseFailString(inputData);
                }
                // 5. Response Data
                log.Debug("[企業加值 Response] Data(length:" + responseString.Length + "):" + responseString);
                responseBytes = Encoding.ASCII.GetBytes(responseString);
                //check client connect state
                if (context.Response.IsClientConnected)
                {
                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);//return  
                }
                else
                {
                    log.Error(m => m("[企業加值]Client disConnect: {0}", responseString));
                }
            }
            else
            {
                //request not defined format
                log.Debug("Request Error");
                //context.Response.ContentType = "text/html";
                //context.Response.Write("<script>alert('Request Error');</script>");
                context.Response.OutputStream.Write(System.Text.Encoding.ASCII.GetBytes("Request Error"), 0, 13);
            }
            timer.Stop();
            log.Debug("[企業加值]End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency).ToString("f3") + "s)");
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            //context.Response.End();//此段會造成以下的Statement不執行
            context.ApplicationInstance.CompleteRequest();//引發EndRequest事件來結束連線
        }

        /// <summary>
        /// 企業自動加值後端異常回應通用格式
        /// </summary>
        /// <param name="inputData">Reqeust 電文</param>
        /// <returns>異常回應通用格式</returns>
        private string GetResponseFailString(string inputData)
        {
            //Com_Type + 原始電文 + Return Code + 原始電文(142 bytes)
            string responseString = Response_Com_Type + inputData.Substring(4, 40) + Response_Generic_Error_ReturnCode + inputData.Substring(50, 92);

            return responseString;
        }

        /// <summary>
        /// Send Reuqest POCO to Center AP and receive response POCO
        /// </summary>
        /// <param name="request">Company自動加值請求物件</param>
        /// <returns>Company自動加值回應物件</returns>
        private CLOL_Soc_Req SendAndReceiveFromAP(CLOL_Soc_Req request, string serviceName)
        {
            CLOL_Soc_Req response = null;
            string requestStr = null;
            byte[] requestBytes = null;
            string responseString = null;
            byte[] responseBytes = null;
            string serverUri = null;
            string errMsg = null;
            NameValueCollection headers = null;
            //*********************************
            //取得連線後台的WebConfig設定資料
            serverUri = ConfigGetter.GetValue(serviceName);
            log.Debug(m => { m.Invoke(serviceName + ":" + serverUri); });
            if (serverUri == null)
            {
                log.Error("要連結的目的地設定資料不存在:" + serviceName);
                return null;
            }
            //*********************************
            try
            {
                //UTF8(JSON(POCO))=>byte array and send to AP
                requestStr = JsonConvert.SerializeObject(request);
                requestBytes = Encoding.UTF8.GetBytes(requestStr);//WebAPI service used UTF8

                log.Debug(m => m("[企業加值]開始送出Request : Uri({0}) data: {1}", serverUri, requestStr));
                headers = new NameValueCollection();
                headers.Add("Content-Type", "application/json");//因應後台WebAPI服務格式要求
                responseBytes = Client.GetResponse(serverUri, "POST", out errMsg, 10000, headers, requestBytes);
                if (responseBytes != null)
                {
                    responseString = Encoding.UTF8.GetString(responseBytes);
                    log.Debug(m => m("[企業加值]Response JsonString:{0}", responseString));
                    response = JsonConvert.DeserializeObject<CLOL_Soc_Req>(responseString);
                }
                else
                {
                    log.Error(m => { m.Invoke("取得回應異常: {0}", errMsg); });
                }

            }
            catch (Exception ex)
            {
                log.Error("[SendAndReceiveFromAP] Error:" + ex.Message + "\n " + ex.StackTrace);
            }
            return response;
        }

        /// <summary>
        /// Company自動加值請求電文字串轉自動加值請求物件(要傳給後端AP用的)
        /// </summary>
        /// <param name="request">Company自動加值請求電文字串(ASCII)</param>
        /// <returns>Company自動加值請求物件</returns>
        private CLOL_Soc_Req ParseRequestString(string request)
        {
            CLOL_Soc_Req toAPObject = null;

            //文件格式參考: iCash2@iBon_Format_20150826(內部使用).xlsx
            if (request.Length != CompanyAutoLoadLength)
            {
                log.Debug("[企業加值]Request字串長度不符:" + request.Length);
                return null;
            }
            else if (!request.Substring(0, 4).Equals(Request_Com_Type))
            {
                log.Debug("[企業加值]Request通訊種別不符:" + request.Substring(0, 4));
                return null;
            }

            try
            {
                toAPObject = new CLOL_Soc_Req
                {
                    COM_TYPE = request.Substring(0, 4),                 //0~3     //0631:通訊種別
                    CHANNEL_TYPE = request.Substring(4, 3),             //4~6     //SET:通路別
                    STORE_NO = request.Substring(7, 8).Remove(0, 2),    //9~14    //123456:店號  //8碼取尾6碼
                    POS_REGNO = request.Substring(15, 3).Remove(0, 1),  //16~17   //11:POS機編號 //3碼取尾2碼
                    POS_SEQNO = request.Substring(18, 8),               //18~25   //12345678:Pos交易序號
                    CASHIER_NO = request.Substring(26, 4),              //26~29   //9999:收銀員編號
                    DATE_TIME = request.Substring(30, 14),              //30~43   //yyyyMMddHHmmss:交易日期
                    SW = request.Substring(44, 6),                      //44~49   //000000:中心端回應碼
                    TERMINAL_ID = request.Substring(50, 16),            //50~65   //8600000000000000:Terminal ID
                    SAM_TSN = request.Substring(66, 6),                 //66~71   //012345:端末交易序號
                    CARD_NO = request.Substring(72, 16),                //72~87   //5817000012345678:卡號
                    CARD_LSN = request.Substring(88, 8),                //88~95   //12345678:加值序號+1
                    AMOUNT = request.Substring(96, 8),                  //96~103  //00000500:交易金額
                    SN = request.Substring(104, 8),                     //104~111 //00000000:交易序號
                    MAC = request.Substring(112, 8),                    //112~119 //1A2B3C4D:交易驗證碼
                    ICC_BL = request.Substring(120, 8),                 //120~127 //卡片餘額
                    SAM_OSN = request.Substring(128, 14)                //128~141 //00000000000000:端末序號: IBON00000000(12)+中心端取號(2) //2015-08-27 新增
                };
            }
            catch (Exception ex)
            {
                log.Error("[企業加值]轉換Request物件失敗:" + ex.Message + "\n " + ex.StackTrace);
            }
            return toAPObject;
        }

        /// <summary>
        /// 比對並轉換Request和Response物件,請求和回應是同一組則out電文字串,不同out null
        /// </summary>
        /// <param name="request">RequestPOCO</param>
        /// <param name="response">ResponsePOCO</param>
        /// <param name="requestString">RequestString</param>
        /// <param name="responseString"></param>
        /// <returns>成功out出response字串/失敗out null</returns>
        private bool ParseResponseString(CLOL_Soc_Req request, CLOL_Soc_Req response, string requestString, out string responseString)
        {
            if (!String.IsNullOrEmpty(response.SW))
            {
                //依文件規格: iCash2@iBon_Format_20150826(內部使用).xlsx
                //********************************************
                //Request部份資料混合Response資料(只改通訊種別,中心端回應碼,TerminalID,端末交易序號,交易金額,交易序號,端末序號)
                responseString = Response_Com_Type +                                    //0~3       //Com_Type : 0632
                                 requestString.Substring(4, 40) +                       //4~43
                                 response.SW +                                          //44~49     //Return Code
                                 (requestString.Substring(50, 2) + response.SAM_OSN) +  //50~65    //"16" + Sam_OSN(14 bytes)
                                 response.SAM_TSN +                                     //66~71     //端末交易序號
                                 requestString.Substring(72, 24) +                      //72~95
                                 response.AMOUNT.PadLeft(8, '0') +                      //96~103    //交易金額
                                 response.SN.PadLeft(8, '0') +                          //104~111   //交易序號
                                 requestString.Substring(112, 16) +                     //112~127
                                 response.SAM_OSN;                                      //128~141   //端末序號: IBON00000000(12)+中心端取號(2) //2015-08-27 新增
                return true;
            }
            else
            {
                responseString = null;
                return false;
            }
        }

        /// <summary>
        /// Get request string from HttpContext Input Stream
        /// </summary>
        /// <param name="context">current HttpContext</param>
        /// <param name="encoding">要使用的編碼方式</param>
        /// <returns>電文字串(已編碼)</returns>
        private static string GetStringFromInputStream(HttpContext context, Encoding encoding)
        {
            StreamReader sr = new StreamReader(context.Request.InputStream, encoding);
            string result = sr.ReadToEnd();
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

        /// <summary>
        /// Get request byte array from HttpContext Input Stream
        /// </summary>
        /// <param name="context">current HttpContext</param>
        /// <returns>電文陣列(未編碼)</returns>
        private static byte[] GetBytesFromInputStream(HttpContext context)
        {
            byte[] buffer = new byte[0x1000];
            int readLength = context.Request.InputStream.Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, readLength);
            return buffer;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
