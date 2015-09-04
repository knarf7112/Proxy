using System;
using System.Text;
//
using Common.Logging;
using System.Web;
using ALCommon;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace Proxy
{
    /// <summary>
    /// TxLog Handler
    /// </summary>
    public class TxLogHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TxLogHandler));

        /// <summary>
        /// 要從web config檔內讀取的資料名稱(TxLog寫入成功時使用的連線設定)
        /// </summary>
        private static readonly string TxLogServiceName = "TxLogService";
        /// <summary>
        /// 要從web config檔內讀取的資料名稱(TxLog寫入失敗時[沖正]使用的連線設定)
        /// </summary>
        private static readonly string ReversalTxLogServiceName = "ReversalTxLogService";
        /// <summary>
        /// used to lock dicApConfig
        /// </summary>
        private static object lockObj = new object();
        /// <summary>
        /// 規格指定的電文長度
        /// </summary>
        private static readonly int TxlogLength = 397;
        /// <summary>
        /// 此服務的請求電文通訊種別(4 bytes)
        /// </summary>
        private static readonly string Request_Com_Type = "0641";
        /// <summary>
        /// 此服務的回應電文通訊種別(4 bytes)
        /// </summary>
        private static readonly string Response_Com_Type = "0642";
        /// <summary>
        /// 通用後台AP錯誤Return Code(6 bytes)
        /// </summary>
        private static readonly string Response_Generic_Error_ReturnCode = "990001";
        /// <summary>
        /// 正常交易的Return Code(用來比對TxLog內卡機回傳的Return Code)
        /// </summary>
        private static readonly string TxLogInnerReturnCode_OK = "00000000";

        /// <summary>
        /// request in
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            string responseString = null;
            byte[] responseBytes = null;
            ALTxlog_Domain request = null;
            ALTxlog_Domain response = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            log.Debug("[AutoLoadTxLog][UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent);
            // 1. get request dat from input stream by ASCII
            string inputData = GetStringFromInputStream(context, Encoding.ASCII);
            log.Debug("[AutoLoadTxLog Request] Data(length:" + inputData.Length + "):" + inputData);

            // 2. Parseing request Data to request POCO
            request = ParseRequestString(inputData);

            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            if (request != null)
            {
                // 3. check TxLog Return Code
                if (HasReversal(request))
                {
                    // 4. Connect TxLog Service and Send TxLog POCO then Response TxLog POCO
                    response = SendAndReceiveFromAP(request, ReversalTxLogServiceName);
                }
                else
                {
                    // 4. Connect Reversal TxLog Service and Send TxLog POCO then Response TxLog POCO
                    response = SendAndReceiveFromAP(request, TxLogServiceName);
                }
                // 5. 若後端AP回應或轉換比對Request和Response有問題時
                if (response == null || !ParseResponseString(request, response, inputData, out responseString))
                {
                    //後端無回應(後端異常)
                    responseString = GetResponseFailString(inputData);
                }
                // 6. Response Data
                log.Debug("[AutoLoadTxLog Response] Data(length:" + responseString.Length + "):" + responseString);
                responseBytes = Encoding.ASCII.GetBytes(responseString);
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);//return 
            }
            else
            {
                //request not defined format
                log.Debug("Request Error");
                context.Response.OutputStream.Write(System.Text.Encoding.ASCII.GetBytes("Request Error"), 0, 13);
            }
            timer.Stop();
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            //context.Response.Close();//異常:會產生2~3個request進入並且無法正常獲得Response
            log.Debug("[AutoLoadTxLog]End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency).ToString("f3") + "s)");
            context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// 檢查TxLog是否需要沖正
        /// </summary>
        /// <param name="request">TxLog物件</param>
        /// <returns>要沖正/不沖正</returns>
        private bool HasReversal(ALTxlog_Domain request)
        {
            //依據文件 iCash2@iBon_Format_20150826(內部使用).xlsx
            string returnCode = request.TXLOG.Substring(16, 8);//直接取TxLog(length:288)內的ReturnCode欄位資料
            string trans_Type = request.TXLOG.Substring(0, 2); //"74" or "75"
            log.Debug(m => { m.Invoke("卡機回傳的ReturnCode:" + returnCode + " 交易類型:" + trans_Type); });
            //自動加值交易是否OK
            if (TxLogInnerReturnCode_OK == returnCode)
            {
                //不沖正
                return false;
            }
            else
            {
                //要沖正
                return true;
            }
        }

        /// <summary>
        /// 自動加值Txlog請求電文字串轉自動加值Txlog請求物件(要傳給後端AP用的)
        /// </summary>
        /// <param name="request">自動加值Txlog請求電文字串(ASCII)</param>
        /// <returns>自動加值Txlog請求物件</returns>
        private ALTxlog_Domain ParseRequestString(string request)
        {
            ALTxlog_Domain toAPObject = null;

            //文件格式參考: iCash2@iBon_Format_20150826(內部使用).xlsx
            if (request.Length != TxlogLength)
            {
                log.Debug("[AutoLoadTxLog]Request字串長度不符:" + request.Length);
                return null;
            }
            else if (!request.Substring(0, 4).Equals(Request_Com_Type))
            {
                log.Debug("[AutoLoadTxLog]Request通訊種別不符:" + request.Substring(0, 4));
                return null;
            }

            try
            {
                toAPObject = new ALTxlog_Domain
                {
                    COM_TYPE = request.Substring(0, 4),                 //0~3     //0631:通訊種別
                    MERC_FLG = request.Substring(4, 3),                 //4~6     //SET:通路別
                    STORE_NO = request.Substring(7, 8).Remove(0, 2),    //9~14    //123456:店號   //2015-09-01 8碼取後面6碼
                    REG_ID = request.Substring(15, 3).Remove(0, 1),     //16~17   //12:POS機編號  //2015-09-01 3碼取後面2碼
                    POS_SEQNO = request.Substring(18, 8),               //18~25   //Pos交易序號
                    TXLOG_RC = request.Substring(44, 6),                //44~49   //中心端回應碼
                    READER_ID = request.Substring(50, 16),              //50~55   //Terminal ID
                    TXLOG = request.Substring(109, 288)                 //109~396  //66 + (331-288) Txlog(去掉Txlog的header:43 bytes)
                };
            }
            catch (Exception ex)
            {
                log.Error("[AutoLoadTxLog]轉換Request物件失敗:" + ex.Message + "\n " + ex.StackTrace);
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
        private bool ParseResponseString(ALTxlog_Domain request, ALTxlog_Domain response, string requestString, out string responseString)
        {
            if (!String.IsNullOrEmpty(response.TXLOG_RC))
            {
                //依文件規格 iCash2@iBon_Format_20150826(內部使用).xlsx
                //Request部份資料混合Response資料(只改通訊種別,中心端回應碼)
                responseString = Response_Com_Type +                //0~3 //Com_Type : 0642
                                 requestString.Substring(4, 40) +   //4~43
                                 response.TXLOG_RC;                 //44~49
                return true;
            }
            else
            {
                responseString = null;
                return false;
            }
        }

        /// <summary>
        /// Txlog後端異常回應通用格式
        /// </summary>
        /// <param name="inputData">Reqeust 電文</param>
        /// <returns>異常回應通用格式</returns>
        private string GetResponseFailString(string inputData)
        {
            //Com_Type + 原始電文 + Return Code (4+40+6=50 bytes)
            string responseString = Response_Com_Type + inputData.Substring(4, 40) + Response_Generic_Error_ReturnCode;

            return responseString;
        }

        /// <summary>
        /// Send Reuqest POCO to Center AP and receive response POCO
        /// </summary>
        /// <param name="request">自動加值請求物件</param>
        /// <returns>自動加值回應物件</returns>
        private ALTxlog_Domain SendAndReceiveFromAP(ALTxlog_Domain request, string serviceName)
        {
            ALTxlog_Domain response = null;
            string requestStr = null;
            byte[] requestBytes = null;
            string responseString = null;
            byte[] responseBytes = null;
            string serverConfig = null;
            string ip = null;
            int port = -1;
            int sendTimeout = -1;
            int receiveTimeout = -1;
            string[] configs = null;
            //*********************************
            //取得連線後台的WebConfig設定資料
            serverConfig = ConfigGetter.GetValue(serviceName);
            log.Debug(m => { m.Invoke(serviceName + ":" + serverConfig); });
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
                log.Error("要連結的目的地設定資料不存在:" + serviceName);
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
                        log.Debug(m => m("[AutoLoadTxLog]Request JsonString({0}): {1}", serverConfig, requestStr));
                        requestBytes = Encoding.UTF8.GetBytes(requestStr);//Center AP used UTF8
                        responseBytes = connectToAP.SendAndReceive(requestBytes);
                        if (responseBytes != null)
                        {
                            responseString = Encoding.UTF8.GetString(responseBytes);
                            response = JsonConvert.DeserializeObject<ALTxlog_Domain>(responseString);
                        }
                        log.Debug(m => { m.Invoke("[AutoLoadTxLog]Response JsonString: {0}", ((responseBytes == null) ? "null" : responseString)); });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(m => m("後台({0})連線異常:{1}", serverConfig, ex.Message));
            }
            return response;
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
