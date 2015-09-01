using System;
//
using System.Web;
using Common.Logging;
using System.Diagnostics;
using System.Configuration;
using System.IO;

namespace Proxy
{
    /// <summary>
    /// TxLog Upload Handler
    /// </summary>
    public class TxLogFileUpLoadHandler : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TxLogFileUpLoadHandler));
        /// <summary>
        /// TxLog WebConfig Setting Key
        /// </summary>
        private static readonly string TxLog_Config_Key = "TxLogStoragePath";
        /// <summary>
        /// 要取得Uri上QueryString的名稱
        /// </summary>
        private static readonly string TxLogQueryStringName = "FileName";
        /// <summary>
        /// 存放TxLog的資料夾路徑
        /// </summary>
        private static string TxLog_Storage_Path;

        /// <summary>
        /// Request in
        /// </summary>
        /// <param name="context">HttpContext</param>
        public void ProcessRequest(HttpContext context)
        {
            string fileName = null;
            string responseString = "";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                //檢視Request屬性數據
                //ReflectionAllPropertyValue(context.Request);
                //1.取得檔案名稱
                if (!GetTxLogFileNameFromUri(context.Request, TxLogQueryStringName, out fileName))
                {
                    responseString = "Get FileName Failed!";
                    return;
                }
                //2.檢查TxLog存放路徑值並設定
                if (String.IsNullOrEmpty(TxLog_Storage_Path))
                {
                    Set_TxLog_Storage_Path();
                }
                //3.檢查今日的資料夾
                fileName = CreateTodayFolder(TxLog_Storage_Path) + fileName;
                //4.TxLog檔案讀取並寫入指定路徑
                if (SaveRequestBodyAsFile(context.Request, fileName))
                {
                    responseString = "document upload complete";
                }
                else
                {
                    responseString = "document upload failed";
                }
                //***********cover the old header
                //context.Response.AppendHeader("Content-Encoding", "utf8");
                //context.Response.AppendHeader("Server", "Qoo Sytem");
                //context.Response.Headers.Remove("X-AspNet-Version");//無法移除
                //context.Response.Headers.Remove("X-SourceFiles");//無法移除
                //****************************************
            }
            catch (Exception ex)
            {
                log.Error("TxLogUpload異常:" + ex.Message + "\n " + ex.StackTrace);
                responseString = "document upload failed!!!";
            }
            finally
            {
                //clear response body content
                context.Response.ClearContent();
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //context.Response.Status = "404 NotFound";
                context.Response.ContentType = "text/plain";
                context.Response.Write(responseString);
                log.Debug(m => m("Response:" + responseString));
                timer.Stop();
                context.Response.Flush();
                //context.Response.Close();//(Chrome)用此方法釋放資源會造成多個Request再發送且Client端無法取得資料,但是用FireFox沒問題
                context.Response.OutputStream.Close();
                log.Debug(m => { m.Invoke("[TxLogFileUpLoad]End Response (TimeSpend:" + ((decimal)timer.ElapsedTicks / Stopwatch.Frequency).ToString("f3") + "s)"); });
                //close client connection
                context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// 取得Request上URI的指定QueryString
        /// </summary>
        /// <param name="request">HttpRequest object</param>
        /// <param name="queryStringKey">specified query string</param>
        /// <param name="fileName">回傳指定QueryString的值</param>
        /// <returns>取值成功/取值失敗</returns>
        private bool GetTxLogFileNameFromUri(HttpRequest request, string queryStringKey, out string fileName)
        {
            fileName = null;
            //將URL上的字串QueryString部分重新作編碼(UTF8);可以解中文字的QueryString
            //System.Diagnostics.Debug.WriteLine("Decode QueryString:\t\t\t" + HttpContext.Current.Server.UrlDecode(request.QueryString.Get(0)));
            if (!String.IsNullOrEmpty(request.QueryString[queryStringKey]))
            {
                fileName = request.QueryString[queryStringKey].ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 設定TxLog檔案存放路徑的靜態欄位值(TxLog_Storage_Path)
        /// </summary>
        private void Set_TxLog_Storage_Path()
        {
            log.Debug(m => m("開始載入Web.Config的AppSettings設定檔"));

            try
            {
                TxLog_Storage_Path = ConfigurationManager.AppSettings[TxLog_Config_Key];
                log.Debug(m => m("TxLog檔案存放路徑:" + (String.IsNullOrEmpty(TxLog_Storage_Path) ? "null" : TxLog_Storage_Path)));
            }
            catch (Exception ex)
            {
                log.Debug(m => { m.Invoke("Web設定檔載入資料錯誤:" + ex.Message + "\n " + ex.StackTrace); });
            }
        }

        /// <summary>
        /// 檢查(若無則建立)今日的資料夾是否存在
        /// </summary>
        /// <param name="path">指定的根目錄路徑</param>
        /// <returns>根目錄路徑 + 今日資料夾</returns>
        private string CreateTodayFolder(string path)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string todayFolderPath = null;
            if (!Directory.Exists(path + date))
            {
                try
                {
                    Directory.CreateDirectory(path + date);
                }
                catch (Exception ex)
                {
                    log.Debug(m => { m.Invoke("建立今日的資料夾失敗:" + ex.Message); });
                    throw new Exception("建立今日的資料夾失敗:" + ex.Message);
                }
            }
            todayFolderPath = path + date + @"\";
            return todayFolderPath;
        }


        /// <summary>
        /// 取得Request內content的數據並寫入指定路徑
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool SaveRequestBodyAsFile(HttpRequest request, string filePath)
        {
            bool result = false;
            System.Collections.Generic.Queue<byte> buffer = null;
            System.IO.Stream requestStream = null;
            byte[] data = null;
            int readByte = -1;

            //檢查檔案路徑
            if (!CheckFilePath(filePath))
            {
                log.Error(m => { m.Invoke("存檔路徑不存在:" + filePath); });
                return false;
            }
            //開始寫檔
            log.Debug(m => { m.Invoke("開始寫檔: " + filePath); });
            using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                buffer = new System.Collections.Generic.Queue<byte>();
                requestStream = request.InputStream;
                try
                {
                    //read data
                    while ((readByte = requestStream.ReadByte()) > -1)
                    {
                        buffer.Enqueue((byte)readByte);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(m => { m.Invoke("Request InputStream Read Error:" + ex.Message + "\n " + ex.StackTrace); });
                }
                finally
                {
                    requestStream.Close();
                }
                //cast to byte array
                data = buffer.ToArray();
                //write byte array in file
                fs.Write(data, 0, data.Length);
                fs.Flush();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 檢查檔案路徑是否存在
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        /// <returns>存在/不存在</returns>
        private bool CheckFilePath(string filePath)
        {
            string directory = null;
            bool result = false;

            try
            {
                directory = System.IO.Path.GetDirectoryName(filePath);
                result = System.IO.Directory.Exists(directory);
            }
            catch (Exception ex)
            {
                log.Error(m => { m.Invoke("[CheckFilePath] Error:" + ex.Message); });
            }
            return result;
        }

        /// <summary>
        /// 用來看進入物件的所有有數據的屬性名稱與值
        /// </summary>
        /// <param name="obj">Reflected object</param>
        private void ReflectionAllPropertyValue(object obj)
        {
            try
            {
                int count = 0;
                object[] index = null;
                //get all properties from object
                System.Reflection.PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (var property in properties)
                {
                    index = null;
                    //if the property is indexer and has length {ex: System.String [Item]}
                    //ref:http://stackoverflow.com/questions/6156577/targetparametercountexception-when-enumerating-through-properties-of-string
                    if (property.GetIndexParameters().Length > 0)
                    {
                        index = new object[] { "0" };
                    }
                    //get property value and casting to string
                    string value = property.GetValue(obj, index) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        System.Diagnostics.Debug.WriteLine(count++ + ":" + property.Name + ":" + value);
                        //log.Error(m => { m.Invoke(count++ + ":" + property.Name + ":" + value); });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(m => { m.Invoke("Reflection Error:" + ex.Message); });
            }
        }

        /// <summary>
        /// reusable
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
