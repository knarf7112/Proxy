<%@ WebHandler Language="C#" Class="AutoLoadHandler" %>

using System;
using System.Web;
using Common.Logging;
using System.IO;
using Newtonsoft.Json;
using ALCommon;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Proxy;
using System.Diagnostics;

public class AutoLoadHandler : IHttpHandler {

    private static readonly ILog log = LogManager.GetLogger(typeof(AutoLoadHandler));
    /// <summary>
    /// 要從web config檔內讀取的資料名稱
    /// </summary>
    private static readonly string ServiceName = "AutoLoadService";
    /// <summary>
    /// used to lock dicApConfig
    /// </summary>
    private static object lockObj = new object();
    /// <summary>
    /// 此服務的請求電文通訊種別(4 bytes)
    /// </summary>
    private static readonly string Request_Com_Type = "0631";
    /// <summary>
    /// 此服務的回應電文通訊種別(4 bytes)
    /// </summary>
    private static readonly string Response_Com_Type = "0632";
    /// <summary>
    /// 自動加值電文長度
    /// </summary>
    private static readonly int AutoLoadLength = 164;
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
        AL2POS_Domain request = null;
        AL2POS_Domain response = null;
        Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        log.Debug("[AutoLoad][UserIP]:" + context.Request.UserHostAddress + "\n UserAgent:" + context.Request.UserAgent);
        // 1. get request dat from input stream by ASCII
        string inputData = GetStringFromInputStream(context, Encoding.ASCII);
        log.Debug("[AutoLoad Request] Data(length:" + inputData.Length + "):" + inputData);

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
                log.Debug(m => { m.Invoke("後端回應:" + (response == null).ToString()); });
                //後端無回應(後端異常)
                responseString = GetResponseFailString(inputData);
            }
            // 5. Response Data
            log.Debug("[AutoLoad Response] Data(length:" + responseString.Length + "):" + responseString);
            responseBytes = Encoding.ASCII.GetBytes(responseString);
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);//return 
        }
        else
        {
            //request not defined format
            log.Debug("Request Error");
            //context.Response.ContentType = "text/html";
            //context.Response.Write("<script>alert('Request Error');</script>");
            context.Response.OutputStream.Write(System.Text.Encoding.ASCII.GetBytes("Request Error"), 0, 13);
        }
        timer.Start();
        log.Debug("[AutoLoad]End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency).ToString("f3") + "s)");
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
        //context.Response.End();//此段會造成以下的Statement不執行
        context.ApplicationInstance.CompleteRequest();//引發EndRequest事件來結束連線
    }

    /// <summary>
    /// 自動加值後端異常回應通用格式
    /// </summary>
    /// <param name="inputData">Reqeust 電文</param>
    /// <returns>異常回應通用格式</returns>
    private string GetResponseFailString(string inputData)
    {
        //Com_Type + 原始電文 + Return Code + 原始電文(164 bytes)
        string responseString = Response_Com_Type + inputData.Substring(4, 40) + Response_Generic_Error_ReturnCode + inputData.Substring(50, 114);

        return responseString;
    }

    /// <summary>
    /// Send Reuqest POCO to Center AP and receive response POCO
    /// </summary>
    /// <param name="request">自動加值請求物件</param>
    /// <returns>自動加值回應物件</returns>
    private AL2POS_Domain SendAndReceiveFromAP(AL2POS_Domain request, string serviceName)
    {
        AL2POS_Domain response = null;
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
                    log.Debug(m => m("[AutoLoad]Request JsonString({0}): {1}", serverConfig, requestStr));
                    requestBytes = Encoding.UTF8.GetBytes(requestStr);//Center AP used UTF8
                    responseBytes = connectToAP.SendAndReceive(requestBytes);
                    if (responseBytes != null)
                    {
                        responseString = Encoding.UTF8.GetString(responseBytes);
                        response = JsonConvert.DeserializeObject<AL2POS_Domain>(responseString);
                    }
                    log.Debug(m => { m.Invoke("[AutoLoad]Response JsonString: {0}", ((responseBytes == null) ? "null" : responseString)); });
                }
            }
        }
        catch (Exception ex)
        {
            log.Error("後台連線異常:" + ex.StackTrace);
        }
        return response;
    }

    /// <summary>
    /// 自動加值請求電文字串轉自動加值請求物件(要傳給後端AP用的)
    /// </summary>
    /// <param name="request">自動加值請求電文字串(ASCII)</param>
    /// <returns>自動加值請求物件</returns>
    private AL2POS_Domain ParseRequestString(string request)
    {
        AL2POS_Domain toAPObject = null;

        //文件格式參考: iCash2@iBon_Format_20150826(內部使用).xlsx
        if (request.Length != AutoLoadLength)
        {
            log.Debug("[AutoLoad]Request字串長度不符:" + request.Length);
            return null;
        }
        else if (!request.Substring(0, 4).Equals(Request_Com_Type))
        {
            log.Debug("[AutoLoad]Request通訊種別不符:" + request.Substring(0, 4));
            return null;
        }

        try
        {
            toAPObject = new AL2POS_Domain
            {
                COM_TYPE = request.Substring(0, 4),                 //0~3     //0631:通訊種別
                MERC_FLG = request.Substring(4, 3),                 //4~6     //SET:通路別
                STORE_NO = request.Substring(7, 8).Remove(0, 2),    //9~14    //123456:店號   //2015-09-01 8碼取後面6碼
                REG_ID = request.Substring(15, 3).Remove(0, 1),     //16~17   //12:POS機編號  //2015-09-01 3碼取後面2碼
                AL_TRANSTIME = request.Substring(30, 14),           //30~43   //yyyyMMddHHmmss:交易日期
                AL2POS_RC = request.Substring(44, 6),               //44~49   //000000:中心端回應碼
                READER_ID = request.Substring(50, 16),              //50~65   //8600000000000000:Terminal ID
                SAM_TSN = request.Substring(66, 6),                 //66~71   //000000:端末交易序號
                ICC_NO = request.Substring(72, 16),                 //72~87   //5817000012345678:卡號
                AL_AMT = Convert.ToInt32(request.Substring(96, 8)), //96~103  //00000500:交易金額
                AL2POS_SN = request.Substring(120, 8),              //120~127 //00000000:交易序號
                AL_RRN = request.Substring(136, 12),                //136~147 //5+365+00+123456:RRN
                AL_ENABLE = request.Substring(149, 1),              //149~149 // " " or "N" : 卡片自動加值啟用代碼 
                SAM_OSN = request.Substring(150, 14),               //150~163 //端末序號:IBON00000000(12) + 中心端取號(2)  
            };
        }
        catch (Exception ex)
        {
            log.Error("[AutoLoad]轉換Request物件失敗:" + ex.StackTrace);
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
    private bool ParseResponseString(AL2POS_Domain request, AL2POS_Domain response, string requestString, out string responseString)
    {
        if (request.COM_TYPE == response.COM_TYPE &&
            request.MERC_FLG == response.MERC_FLG &&
            //request.AL_TRANSTIME == response.AL_TRANSTIME && //因後台AP沒回傳交易時間所以不能比對
            request.ICC_NO == response.ICC_NO &&
            request.STORE_NO == response.STORE_NO &&
            request.AL_AMT == response.AL_AMT)
        {
            //依文件規格 iCash2@iBon_Format_20150826(內部使用).xlsx
            //********************************************
            //string response_SAM_TSN = (Convert.ToInt32(requestString.Substring(66, 6)) - 1).ToString("D6");
            //string response_Card_LSN = (Convert.ToInt32(requestString.Substring(88, 8)) + 1).ToString("D8");
            //Request部份資料混合Response資料
            //responseString = requestString.Substring(0, 43) +   //0~43
            //                 response.AL2POS_RC +               //44~49
            //                 requestString.Substring(50, 16) +  //50~65
            //                 response_SAM_TSN +                 //66~71
            //                 response.ICC_NO +                  //72~87
            //                 response_Card_LSN +                //88~95
            //                 requestString.Substring(96, 24) +  //96~119
            //                 response.AL2POS_SN +               //120~127
            //                 requestString.Substring(128, 20);  //128~147
            //********************************************
            //Request部份資料混合Response資料(只改通訊種別,中心端回應碼,TerminalID,交易序號,RRN,虛擬SAM_TSN,虛擬Sam_OSN)
            responseString = Response_Com_Type +                                    //0~3      //Com_Type : 0632
                             requestString.Substring(4, 40) +                       //4~43
                             response.AL2POS_RC +                                   //44~49    //Return Code: 6 bytes
                             (requestString.Substring(50, 2) + response.SAM_OSN) +  //50~65    //"16" + Sam_OSN(14 bytes)
                             response.SAM_TSN +                                     //66~71    //SAM_TSN: 6 bytes
                             requestString.Substring(72, 48) +                      //72~119
                             response.AL2POS_SN.PadLeft(8, '0') +                   //120~127 //交易序號: 8 bytes
                             requestString.Substring(128, 8) +                      //128~135  
                             response.AL_RRN +                                      //136~147  //RRN: 12 bytes
                             requestString.Substring(148, 2) +                      //148~149
                             response.SAM_OSN;                                      //150~163  //Sam_OSN: 14 bytes
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