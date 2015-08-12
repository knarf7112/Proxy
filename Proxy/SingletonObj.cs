using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Proxy.POCO;
using System.Diagnostics;
using System.Configuration;
using Common.Logging;

namespace Proxy
{
    /// <summary>
    /// 存放設定檔用的靜態物件(測試階段)
    /// </summary>
    public static class SingletonObj
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SingletonObj));

        private static IDictionary<string, ServiceConfig> dicAPConfig;

        private static object lockObj = new object();

        static SingletonObj()
        {
            log.Debug((m) => { m.Invoke("執行SingletonObj靜態初始化"); });
            InitialIpConfig();
        }

        //private SingletonObj()
        //{

        //}

        /// <summary>
        /// 取得後台AP服務的連線資訊物件
        /// </summary>
        /// <param name="ServiceConfigName">後台AP服務名稱</param>
        /// <returns>後台AP服務連線資訊</returns>
        public static ServiceConfig GetConfigInstance(string ServiceConfigName)
        {
            log.Debug((m) => { m.Invoke("開始取得設定資料物件:" + ServiceConfigName); });
            if (dicAPConfig == null)
            {
                lock (lockObj)
                {
                    if (dicAPConfig == null)
                    {
                        InitialIpConfig();
                    }
                }
            }
            if (!dicAPConfig.ContainsKey(ServiceConfigName))
            {
                log.Debug((m) => { m.Invoke("資料物件: " + ServiceConfigName + " 不存在"); });
                return null;
            }

            return dicAPConfig[ServiceConfigName];
        }

        /// <summary>
        /// 檢查Web.config的AppSettings內是否有設定後端服務的IP,Port,送出和接收逾時
        /// 並設定到AutoLoadHandler.apIPConfig的字典檔裡暫存
        /// </summary>
        private static void InitialIpConfig()
        {
            log.Debug((m) => { m.Invoke("開始載入Web.Config的AppSettings設定檔"); });
            dicAPConfig = new Dictionary<string, ServiceConfig>();
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
                        dicAPConfig.Add(item, config);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug((m) => { m.Invoke("Web設定檔載入資料錯誤:" + ex.StackTrace); });
            }

        }
    }
}
