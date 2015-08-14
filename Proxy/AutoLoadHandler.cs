﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//
using ALCommon;
using Newtonsoft.Json;
using System.Web;
using Common.Logging;
using System.Configuration;
using Proxy.POCO;
using System.Diagnostics;

namespace Proxy
{
    /// <summary>
    /// 自動加值Handler
    /// </summary>
    public class AutoLoadHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AutoLoadHandler));

        /// <summary>
        /// 連向後台AP的設定資料(IP,Port,SendTimeout,ReceiveTimeout)
        /// Key:設定檔的appSettings裡的Name
        /// </summary>
        private static IDictionary<string, ServiceConfig> dicApConfig;
        /// <summary>
        /// 要從web config檔內讀取的資料名稱
        /// </summary>
        private static readonly string APServiceName = "AutoLoadService";
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
            Stopwatch timer = new Stopwatch();
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
                AL2POS_Domain response = SendAndReceiveFromAP(request);
                // 4. 若後端AP回應或轉換比對Request和Response有問題時
                if (response == null || !ParseResponseString(request, response, inputData, out responseString))
                {
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
                context.Response.OutputStream.Write(System.Text.Encoding.ASCII.GetBytes("Request Error"), 0, 13);//.Write("Test");            
            }
            timer.Start();
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            //context.Response.End();
            context.ApplicationInstance.CompleteRequest();
            log.Debug("[AutoLoad]End Response (TimeSpend:" + (timer.ElapsedTicks / (decimal)System.Diagnostics.Stopwatch.Frequency) + "ms)");
        }

        /// <summary>
        /// 自動加值後端異常回應通用格式
        /// </summary>
        /// <param name="inputData">Reqeust 電文</param>
        /// <returns>異常回應通用格式</returns>
        private string GetResponseFailString(string inputData)
        {
            //Com_Type + 原始電文 + Return Code + 原始電文
            string responseString = "" + inputData.Substring(0, 44) + Response_Generic_Error_ReturnCode + inputData.Substring(50, 98);

            return responseString;
        }

        /// <summary>
        /// Send Reuqest POCO to Center AP and receive response POCO
        /// </summary>
        /// <param name="request">自動加值請求物件</param>
        /// <returns>自動加值回應物件</returns>
        private AL2POS_Domain SendAndReceiveFromAP(AL2POS_Domain request)
        {
            AL2POS_Domain response = null;
            string requestStr = null;
            byte[] requestBytes = null;
            string responseString = null;
            byte[] responseBytes = null;

            //*********************************
            //檢查字典檔是否有設定檔資料
            log.Debug("後台AP資料設定暫存是否存在:" + (dicApConfig == null).ToString());
            if (dicApConfig == null)
            {
                lock (lockObj)
                {
                    if (dicApConfig == null)
                    {
                        InitialIpConfig();
                    }
                }
            }
            else if (!dicApConfig.ContainsKey(APServiceName))
            {
                log.Error("WebConfig的appSettings[" + APServiceName + "] 設定資料不存在");
                return null;
            }
            //*********************************

            //UTF8(JSON(POCO))=>byte array and send to AP
            requestStr = JsonConvert.SerializeObject(request);
            log.Debug("Request JsonString:" + requestStr);
            requestBytes = Encoding.UTF8.GetBytes(requestStr);//Center AP used UTF8
            try
            {
                using (SocketClient.Domain.SocketClient connectToAP = new SocketClient.Domain.SocketClient(dicApConfig[APServiceName].IP, dicApConfig[APServiceName].Port, dicApConfig[APServiceName].SendTimeout, dicApConfig[APServiceName].ReceiveTimeout))
                {
                    log.Debug("開始連線後端AP");
                    if (connectToAP.ConnectToServer())
                    {
                        responseBytes = connectToAP.SendAndReceive(requestBytes);
                        if (responseBytes != null)
                        {
                            responseString = Encoding.UTF8.GetString(responseBytes);
                            log.Debug("Response JsonString:" + responseString);
                            response = JsonConvert.DeserializeObject<AL2POS_Domain>(responseString);
                        }
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
        /// 檢查Web.config的AppSettings內是否有設定後端服務的IP,Port,送出和接收逾時
        /// 並設定到AutoLoadHandler.apIPConfig的字典檔裡暫存
        /// </summary>
        private void InitialIpConfig()
        {
            log.Debug("開始載入Web.Config的AppSettings設定檔");
            AutoLoadHandler.dicApConfig = new Dictionary<string, ServiceConfig>();
            try
            {
                foreach (string item in ConfigurationManager.AppSettings.Keys)
                {
                    //找包含"Service"名稱的當作IP設定資料
                    if (item.IndexOf("Service") > -1)
                    {
                        string[] serviceConfig = ConfigurationManager.AppSettings[item].Split(':');
                        ServiceConfig config = new ServiceConfig()
                        {
                            IP = serviceConfig[0],
                            Port = Convert.ToInt32(serviceConfig[1]),
                            SendTimeout = Convert.ToInt32(serviceConfig[2]),
                            ReceiveTimeout = Convert.ToInt32(serviceConfig[3])
                        };
                        AutoLoadHandler.dicApConfig.Add(item, config);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug("Web設定檔載入資料錯誤:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 自動加值請求電文字串轉自動加值請求物件(要傳給後端AP用的)
        /// </summary>
        /// <param name="request">自動加值請求電文字串(ASCII)</param>
        /// <returns>自動加值請求物件</returns>
        private AL2POS_Domain ParseRequestString(string request)
        {
            AL2POS_Domain toAPObject = null;

            //文件格式參考: iCash2@iBon_Format_20150810.xlsx
            if (request.Length != 148)
            {
                log.Debug("Request字串長度不符:" + request.Length);
                return null;
            }
            else if (!request.Substring(0, 4).Equals(Request_Com_Type))
            {
                log.Debug("Request通訊種別不符:" + request.Substring(0, 4));
                return null;
            }

            try
            {
                toAPObject = new AL2POS_Domain
                {
                    COM_TYPE = request.Substring(0, 4),                 //0~3     //0631:通訊種別
                    MERC_FLG = request.Substring(4, 3),                 //4~6     //SET:通路別
                    STORE_NO = request.Substring(7, 8),                 //7~14    //01234567:店號
                    REG_ID = request.Substring(15, 3),                  //15~17   //000:POS機編號
                    AL_TRANSTIME = request.Substring(30, 14),           //30~43   //yyyyMMddHHmmss:交易日期
                    AL2POS_RC = request.Substring(44, 6),               //44~49   //000000:中心端回應碼
                    READER_ID = request.Substring(50, 16),              //50~65   //8600000000000000:Terminal ID
                    ICC_NO = request.Substring(72, 16),                 //72~87   //5817000012345678:卡號
                    AL_AMT = Convert.ToInt32(request.Substring(96, 8)), //96~103  //00000500:交易金額
                    AL2POS_SN = request.Substring(120, 8),              //120~127 //00000000:交易序號
                    //RRN屬性還未設定 2015-08-10
                };
            }
            catch (Exception ex)
            {
                log.Error("轉換Request物件失敗:" + ex.StackTrace);
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
                //依文件規格 iCash2@iBon_Format_20150810.xlsx
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
                //Request部份資料混合Response資料(只改通訊種別,中心端回應碼,交易序號)
                responseString = Response_Com_Type +                //0~3 //Com_Type : 0632
                                 requestString.Substring(4, 40) +   //4~43
                                 response.AL2POS_RC +               //44~49
                                 requestString.Substring(50, 70) +  //50~119
                                 response.AL2POS_SN +               //120~125
                                 requestString.Substring(128, 20);  //128~147
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
