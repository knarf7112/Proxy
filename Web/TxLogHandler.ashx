<%@ WebHandler Language="C#" Class="TxLogHandler" %>

using System;
using System.Web;
//
using ALCommon;

public class TxLogHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) 
    {
        ALTxlog_Domain request = null;
        string responseString = null;
        byte[] responseBytes = null;
        
        
        
        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}