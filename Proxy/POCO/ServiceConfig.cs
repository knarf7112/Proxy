using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxy.POCO
{
    public struct ServiceConfig
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }
    }
}
