<%@ WebHandler Language="C#" Class="CompanyPrepaidTxLogHandler" %>

using System;
using System.Web;
//
using Proxy;
using Common.Logging;
using System.IO;
using System.Text;
using ALCommon;
using System.Diagnostics;
using Newtonsoft.Json;
using WebHttpClient;
using IBON_TRADE_MANAGER_Lib;
using System.Collections.Specialized;

public class CompanyPrepaidTxLogHandler : IHttpHandler
{

    private static readonly ILog log = LogManager.GetLogger(typeof(CompanyPrepaidTxLogHandler));

    /// <summary>
    /// 要從web config檔內讀取的資料名稱(鎖卡TxLog要回傳的後台Uri)
    /// </summary>
    private static readonly string ServiceName = "CompanyChargeTxLogService";
    /// <summary>
    /// 規格指定的電文長度
    /// </summary>
    private static readonly int TxlogLength = 397;
    /// <summary>
    /// 此服務的請求電文通訊種別(4 bytes)
    /// </summary>
    private static readonly string Request_Com_Type = "0345";
    /// <summary>
    /// 此服務的回應電文通訊種別(4 bytes)
    /// </summary>
    private static readonly string Response_Com_Type = "0346";
    /// <summary>
    /// 通用後台AP錯誤Return Code(6 bytes)
    /// </summary>
    private static readonly string Response_Generic_Error_ReturnCode = "990001";
    /// <summary>
    /// 正常交易的Return Code(用來比對TxLog內卡機回傳的Return Code)
    /// </summary>
    private static readonly string TxLogInnerReturnCode_OK = "00000000";


    public void ProcessRequest(HttpContext context)
    {

        string responseString = null;
        byte[] responseBytes = null;
        TOL_Soc_Req request = null;
        TOL_Soc_Req response = null;
        Stopwatch timer = new Stopwatch();
        timer.Start();

        log.Info(m => { m.Invoke("[企業加值Txlog][UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent); });
        // 1. get request dat from input stream by ASCII
        string inputData = GetStringFromInputStream(context, Encoding.ASCII);
        log.Debug("[企業加值Txlog Request] Data(length:" + inputData.Length + "):" + inputData);

        // 2. Parseing request Data to request POCO
        request = ParseRequestString(inputData);

        context.Response.ContentType = "text/plain";
        context.Response.StatusCode = 200;
        if (request != null)
        {
            // 3. Connect Reversal TxLog Service and Send TxLog POCO then Response TxLog POCO
            response = SendAndReceiveFromAP(request, ServiceName);

            // 4. 若後端AP回應或轉換比對Request和Response有問題時
            if (response == null || !ParseResponseString(request, response, inputData, out responseString))
            {
                //後端無回應(後端異常)
                responseString = GetResponseFailString(inputData);
            }
            // 5. Response Data
            log.Debug("[企業加值Txlog Response] Data(length:" + responseString.Length + "):" + responseString);
            responseBytes = Encoding.ASCII.GetBytes(responseString);
            //check client connect state
            if (context.Response.IsClientConnected)
            {
                context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);//return  
            }
            else
            {
                log.Error(m => m("[CompanyPrepaidTxLog]Client disConnect: {0}", responseString));
            }
        }
        else
        {
            //request not defined format
            log.Debug("Request Error");
            context.Response.OutputStream.Write(System.Text.Encoding.ASCII.GetBytes("Request Error"), 0, 13);
        }
        timer.Start();
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
        log.Debug("[企業加值Txlog]End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency).ToString("f3") + "s)");
        context.ApplicationInstance.CompleteRequest();
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Send Reuqest POCO to Center AP and receive response POCO
    /// </summary>
    /// <param name="request">自動加值請求物件</param>
    /// <returns>自動加值回應物件</returns>
    private TOL_Soc_Req SendAndReceiveFromAP(TOL_Soc_Req request, string serviceName)
    {
        TOL_Soc_Req response = null;
        string requestStr = null;
        byte[] requestBytes = null;
        string responseString = null;
        byte[] responseBytes = null;
        string serverUri = null;
        string errMsg = null;
        NameValueCollection headers = null;
        //string fixedRequestStr = null;
        //string fixedresponseStr = null;
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
            requestBytes = Encoding.UTF8.GetBytes(requestStr);//Center AP used UTF8
            //fixedRequestStr = requestStr.Replace("{", "{{").Replace("}", "}}");//修正log4net的JSON轉換問題 //ref:http://chwilliamson.me.uk/article/CommonLoggingTraceListener-to-Log4Net-FormatException
            log.Debug(m => m("[企業加值Txlog]開始送出Request : Uri({0}) data: {1}", serverUri, requestStr));
            headers = new NameValueCollection();
            headers.Add("Content-Type", "application/json");//因應後台WebAPI服務格式要求
            //送出Request請求與請求數據並等待回應數據
            responseBytes = Client.GetResponse(serverUri, "POST", out errMsg, 10000, headers, requestBytes);
            if (responseBytes != null)
            {
                responseString = Encoding.UTF8.GetString(responseBytes);
                log.Debug(m => m("[企業加值Txlog]Response JsonString: {0}", responseString));
                response = JsonConvert.DeserializeObject<TOL_Soc_Req>(responseString);
            }
            else
            {
                log.Error(m => { m.Invoke("取得回應異常: {0}", errMsg); });
            }

        }
        catch (Exception ex)
        {
            log.Error("[SendAndReceiveFromAP] Error:" + ex.Message + ex.StackTrace);
        }
        return response;
    }


    /// <summary>
    /// Company自動加值Txlog請求電文字串轉自動加值Txlog請求物件(要傳給後端AP用的)
    /// </summary>
    /// <param name="request">自動加值Txlog請求電文字串(ASCII)</param>
    /// <returns>自動加值Txlog請求物件</returns>
    private TOL_Soc_Req ParseRequestString(string request)
    {
        TOL_Soc_Req toAPObject = null;

        //文件格式參考: iCash2@iBon_Format_20150826(內部使用).xlsx
        if (request.Length != TxlogLength)
        {
            log.Debug("[企業加值Txlog]Request字串長度不符:" + request.Length);
            return null;
        }
        else if (!request.Substring(0, 4).Equals(Request_Com_Type))
        {
            log.Debug("[企業加值Txlog]Request通訊種別不符:" + request.Substring(0, 4));
            return null;
        }

        try
        {
            toAPObject = new TOL_Soc_Req
            {
                COM_TYPE = request.Substring(0, 4),                 //0~3     //0631:通訊種別
                CHANNEL_TYPE = request.Substring(4, 3),             //4~6     //SET:通路別
                STORE_NO = request.Substring(7, 8).Remove(0, 2),    //9~14    //123456:店號  //8碼取尾6碼
                POS_REGNO = request.Substring(15, 3).Remove(0, 1),  //16~17   //11:POS機編號 //3碼取尾2碼
                POS_SEQNO = request.Substring(18, 8),               //18~25   //12345678:Pos交易序號
                CASHIER_NO = request.Substring(26, 4),              //26~29   //0000:收銀員編號
                DATE_TIME = request.Substring(30, 14),              //30~43   //20150824165959:交易日期(yyyyMMddHHmmss)
                SW = request.Substring(44, 6),                      //44~49   //000000:中心端回應碼
                TERMINAL_ID = request.Substring(50, 16),            //50~55   //Terminal ID
                TxLog = request.Substring(66, 331)                  //66~396  //Txlog(包含Txlog的header + body:43 + 288 bytes)
            };
        }
        catch (Exception ex)
        {
            log.Error("[企業加值Txlog]轉換Request物件失敗:" + ex.StackTrace);
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
    private bool ParseResponseString(TOL_Soc_Req request, TOL_Soc_Req response, string requestString, out string responseString)
    {
        //if (request.COM_TYPE == response.COM_TYPE &&
        //    request.POS_SEQNO == response.POS_SEQNO &&
        //    request.STORE_NO == response.STORE_NO)
        if (!String.IsNullOrEmpty(response.SW))
        {
            //依文件規格 iCash2@iBon_Format_20150826(內部使用).xlsx
            //Request部份資料混合Response資料(只改通訊種別,中心端回應碼)
            responseString = Response_Com_Type +                //0~3 //Com_Type : 0642
                             requestString.Substring(4, 40) +   //4~43
                             response.SW;                       //44~49
            return true;
        }
        else
        {
            responseString = null;
            return false;
        }
    }

    /// <summary>
    /// 企業加值Txlog後端異常回應通用格式
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
        byte[] buffer = new byte[0x1000];//4k
        int readLength = context.Request.InputStream.Read(buffer, 0, buffer.Length);
        Array.Resize(ref buffer, readLength);
        return buffer;
    }

}