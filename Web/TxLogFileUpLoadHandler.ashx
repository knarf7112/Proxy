<%@ WebHandler Language="C#" Class="TxLogFileUpLoadHandler" %>

using System;
using System.Web;

public class TxLogFileUpLoadHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        var rqt = context.Request;
        
        ReflectionAllPropertyValue(rqt);
        //********************************
        if (rqt.QueryString.Count > 0)
        {
            //沒有編碼  直接取字串
            System.Diagnostics.Debug.WriteLine("None Decode QueryString:\t" + rqt.QueryString.Get(0));
            //將URL上的字串QueryString部分重新作編碼(UTF8);可以解中文字的QueryString
            System.Diagnostics.Debug.WriteLine("Decode QueryString:\t\t\t" + context.Server.UrlDecode(rqt.QueryString.Get(0)));
        }
        //********************************
        
        byte[] buffer = new byte[0x1000];
        int readCnt = rqt.InputStream.Read(buffer, 0, buffer.Length);
        Array.Resize(ref buffer,readCnt);
        System.Diagnostics.Debug.WriteLine("Data:" + BitConverter.ToString(buffer));
        System.Diagnostics.Debug.WriteLine("Data:" + System.Text.Encoding.ASCII.GetString(buffer));

        //clear response body content
        context.Response.ClearContent();
        
        context.Response.TrySkipIisCustomErrors = true;
        context.Response.ContentEncoding = System.Text.Encoding.UTF8;
        
        context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
        context.Response.Status = "200 Unauthorized";
        context.Response.ContentType = "text/plain";
        //***********cover the old header  song...
        context.Response.AppendHeader("Content-Encoding", "utf8");
        //context.Response.AppendHeader("Server", "Qoo Sytem");
        //context.Response.Headers.Remove("X-AspNet-Version");//無法移除
        //context.Response.Headers.Remove("X-SourceFiles");//無法移除
        //****************************************
        context.Response.Write("Hello World Yaaaaaa!");
        context.Response.Flush();
        
        context.ApplicationInstance.CompleteRequest();
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
            //get all properties from object
            System.Reflection.PropertyInfo[] properties = obj.GetType().GetProperties();
            
            foreach (var property in properties)
            {
                object[] index = null;
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
                    //display on debug console
                    System.Diagnostics.Debug.WriteLine(count++ + ":" + property.Name + ":" + value);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Reflection Error:" + ex.StackTrace);
        }
    }
    public bool IsReusable {
        get {
            return false;
        }
    }

}