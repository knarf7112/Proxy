<%@ WebHandler Language="C#" Class="AuthHandler" %>

using System;
using System.Web;
//
using Crypto.EskmsAPI;
using System.IO;
using Common.Logging;
using System.Text;
    public class AuthHandler : IHttpHandler
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(AuthHandler));

        public void ProcessRequest(HttpContext context)
        {

            log.Debug("UserIP:" + context.Request.UserHostAddress);
            string inputData = GetStringFromInputStream(context);
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            if (!String.IsNullOrEmpty(inputData))
            {
                log.Debug("Request Data:" + inputData);
                byte[] responseData = DoAuthenticate(inputData);
                //context.Response.OutputStream.Position = 0;
                log.Debug("start Response Out ...");
                context.Response.OutputStream.Write(responseData, 0, responseData.Length);//return 68 bytes
                log.Debug("End Response");
            }
            else
            {
                log.Debug("Test123");
                context.Response.OutputStream.Write(Encoding.ASCII.GetBytes("Test123"), 0, 7);//.Write("Test");
            }
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
            context.Response.End();
            
        }

        private static string GetStringFromInputStream(HttpContext context)
        {
            StreamReader sr = new StreamReader(context.Request.InputStream, System.Text.Encoding.ASCII);
            string result = sr.ReadToEnd();
            return result;
        }

        private static byte[] GetBytesFromInputStream(HttpContext context)
        {
            byte[] buffer = new byte[0x1000];
            int readLength = context.Request.InputStream.Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, readLength);
            return buffer;
        }
        
        private static byte[] DoAuthenticate(string inputData)
        {
            if (inputData.Length != 50)
                return new byte[5] { 0x48, 0x61, 0x20, 0x48, 0x61 };//ha ha
            string keyLabel = "2ICH3F0000" + inputData.Substring(0, 2) + "A";   //0~1   => "32" <= byte{0x33,0x32}
            string KeyVersion = inputData.Substring(2, 2);                      //2~3   => "00" <= byte{0x30,0x30}
            string uid = inputData.Substring(4, 14);                            //4~17  => "04873ABA8D2C80" <= byte{0x04,0x87,0x3A,0xBA,0x8D,0x2C,0x80}
            string enc_RanB = inputData.Substring(18, 32);                      //18~50 => "4EF61041ABE8B0EF8B32A627B19D83AA" <= byte{0x4E,0xF6,0x10,0x41,0xAB,0xE8,0xB0,0xEF,0x8B,0x32,0xA6,0x27,0xB1,0x9D,0x83,0xAA}
            IiBonAuthenticate iBonAuth = new iBonAuthenticate()
            {
                Input_KeyLabel = keyLabel,
                Input_KeyVersion = KeyVersion,
                Input_UID = uid,
                Input_Enc_RanB = enc_RanB,
            };
            //run
            iBonAuth.StartAuthenticate(true);
            byte[] result = new byte[iBonAuth.Output_RanB.Length +
                                     iBonAuth.Output_Enc_RanAandRanBRol8.Length +
                                     iBonAuth.Output_Enc_IVandRanARol8.Length + 4];
            byte[] randAStartIndexBytes = BitConverter.GetBytes(iBonAuth.Output_RandAStartIndex);//4 bytes
            Buffer.BlockCopy(iBonAuth.Output_RanB, 0, result, 0, iBonAuth.Output_RanB.Length);//Copy Random B in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_RanAandRanBRol8, 0, result, iBonAuth.Output_RanB.Length, iBonAuth.Output_Enc_RanAandRanBRol8.Length);//Copy E(RanA || RanBRol8) in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_IVandRanARol8, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length, iBonAuth.Output_Enc_IVandRanARol8.Length);//Copy E(iv,RanARol8) in Result
            Buffer.BlockCopy(randAStartIndexBytes, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length + iBonAuth.Output_Enc_IVandRanARol8.Length, randAStartIndexBytes.Length);//Copy Random A Start Index (4 Bytes)
            log.Debug("RanB:" + BitConverter.ToString(iBonAuth.Output_RanB).Replace("-", ""));
            log.Debug("E(RanA || RanBRol8):" + BitConverter.ToString(iBonAuth.Output_Enc_RanAandRanBRol8).Replace("-", ""));
            log.Debug("E(iv,RanARol8):" + BitConverter.ToString(iBonAuth.Output_Enc_IVandRanARol8).Replace("-", ""));
            log.Debug("Random A Start Index:" + iBonAuth.Output_RandAStartIndex);
            log.Debug("Session Key:" + BitConverter.ToString(iBonAuth.Output_SessionKey).Replace("-", ""));
            return result;
        }
        
        private static byte[] DoAuthenticate(byte[] inputData)
        {
            if (inputData.Length != 24)
                return new byte[5] { 0x48, 0x61, 0x20, 0x48, 0x61 };                        //ha ha
            string keyLabel = "2ICH3F0000" + inputData[0].ToString() + "A";                 //byte[0]
            string KeyVersion = inputData[1].ToString();                                    //byte[1]
            string uid = BitConverter.ToString(inputData, 2, 7).Replace("-","");            //byte[2~8]
            string enc_RanB = BitConverter.ToString(inputData, 9, 16).Replace("-", "");     //byte[9~24]
            IiBonAuthenticate iBonAuth = new iBonAuthenticate()
            {
                Input_KeyLabel = keyLabel,
                Input_KeyVersion = KeyVersion,
                Input_UID = uid,
                Input_Enc_RanB = enc_RanB,
            };
            //run
            iBonAuth.StartAuthenticate(true);
            byte[] result = new byte[iBonAuth.Output_RanB.Length +
                                     iBonAuth.Output_Enc_RanAandRanBRol8.Length +
                                     iBonAuth.Output_Enc_IVandRanARol8.Length + 4];
            byte[] randAStartIndexBytes = BitConverter.GetBytes(iBonAuth.Output_RandAStartIndex);//4 bytes
            Buffer.BlockCopy(iBonAuth.Output_RanB, 0, result, 0, iBonAuth.Output_RanB.Length);//Copy Random B in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_RanAandRanBRol8, 0, result, iBonAuth.Output_RanB.Length, iBonAuth.Output_Enc_RanAandRanBRol8.Length);//Copy E(RanA || RanBRol8) in Result
            Buffer.BlockCopy(iBonAuth.Output_Enc_IVandRanARol8, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length, iBonAuth.Output_Enc_IVandRanARol8.Length);//Copy E(iv,RanARol8) in Result
            Buffer.BlockCopy(randAStartIndexBytes, 0, result, iBonAuth.Output_RanB.Length + iBonAuth.Output_Enc_RanAandRanBRol8.Length + iBonAuth.Output_Enc_IVandRanARol8.Length, randAStartIndexBytes.Length);//Copy Random A Start Index (4 Bytes)
            log.Debug("RanB:" + BitConverter.ToString(iBonAuth.Output_RanB).Replace("-", ""));
            log.Debug("E(RanA || RanBRol8):" + BitConverter.ToString(iBonAuth.Output_Enc_RanAandRanBRol8).Replace("-", ""));
            log.Debug("E(iv,RanARol8):" + BitConverter.ToString(iBonAuth.Output_Enc_IVandRanARol8).Replace("-", ""));
            log.Debug("Random A Start Index:" + iBonAuth.Output_RandAStartIndex);
            log.Debug("Session Key:" + BitConverter.ToString(iBonAuth.Output_SessionKey).Replace("-", ""));
            return result;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }