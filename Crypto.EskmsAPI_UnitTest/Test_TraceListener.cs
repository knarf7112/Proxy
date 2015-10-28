using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Common.Logging;
using System.Diagnostics;

namespace Crypto.EskmsAPI_UnitTest
{
    [TestClass]
    public class Test_TraceListener
    {

        //private static readonly ILog log = LogManager.GetLogger(typeof(Test_TraceListener));

        [TestInitialize]
        public void init()
        {
            
        }

        [TestMethod]
        public void TestMethod1()
        {
            //測試這邊寫debug會不會寫到d:\temp\
            //設定檔在app.config  ==> 結論: OK ,可以攔截到
            Debug.WriteLine("Debug write test hahaha");
            writeDebug(10);
            var qq = Debug.Listeners[1];
            //log.Debug("this is write test");
        }
        
        public void writeDebug(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Debug.WriteLine("[WriteDebug] {0}", i);
            }
        }
        /*
        public override void Write(string message)
        {
            log.Debug(message);
        }

        public override void WriteLine(string message)
        {
            log.Debug(message);
        }
        */
    }
}
