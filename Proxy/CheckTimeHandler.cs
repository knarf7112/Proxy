using System;
using System.Text;
//
using System.Web;
using System.IO;

namespace Proxy
{
    public class CheckTimeHandler : IHttpHandler
    {
        static bool isStart = false;
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //no command
            if (context.Request.QueryString.Count == 0)
            {
                return;
            }

            var start = isStart;
            if (!isStart)
                isStart = true;
            var qq = context;
            byte[] request = null;
            DateTime now = DateTime.Now;
            
            if(context.Request.HttpMethod.ToUpper() == "GET")
            {
                request = Encoding.UTF8.GetBytes(now.ToString("yyyy-MM-dd HH:mm:ss") + " => " + context.Request.QueryString[0] + "\n");
            }
            string path = AppDomain.CurrentDomain.BaseDirectory;
            using(FileStream fs = new FileStream(path + "Log.txt",FileMode.OpenOrCreate,FileAccess.Write))
            {
                fs.Write(request, 0, request.Length);
            }
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/octet-stream";

            using (Stream sw = context.Response.OutputStream)
            {
                byte[] responseArr = Encoding.ASCII.GetBytes(now.ToString("yyyyMMddHHmmss"));
                //now.ToString("yyyyMMddHHmmss")
                sw.Write(responseArr, 0, responseArr.Length);
                sw.Flush();
            }
        }
    }
}
