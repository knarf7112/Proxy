using System.Diagnostics;
using Common.Logging;
namespace Crypto.EskmsAPI_UnitTest
{
    /// <summary>
    /// Debug的Listener轉到Common loggging(需要在app.config作設定監聽的物件)
    /// <system.diagnostics>
    //  <trace>
    //    <listeners>
    //      <add name="traceListener" type="Crypto.EskmsAPI_UnitTest.DebugToLog4Net, Crypto.EskmsAPI_UnitTest"></add>
    //    </listeners>
    //  </trace>
    //</system.diagnostics>
    /// </summary>
    public class DebugToLog4Net : TraceListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DebugToLog4Net));

        public override void Write(string message)
        {
            log.Debug(m => m("{0}",message));
        }

        public override void WriteLine(string message)
        {
            log.Debug(m => m("{0}", message));
        }
    }
}
