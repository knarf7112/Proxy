using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Web;
namespace Proxy
{
    public class AuthenticateHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { throw new NotImplementedException(); }
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
