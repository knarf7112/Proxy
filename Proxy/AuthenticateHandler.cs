using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Web;
namespace Proxy
{
    /// <summary>
    /// 3 Pass Authentication
    /// </summary>
    public class AuthenticateHandler : CardValidationHandler,IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public override void ProcessRequest(HttpContext context)
        {
            
        }
    }
}
