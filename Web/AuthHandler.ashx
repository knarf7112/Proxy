﻿<%@ WebHandler Language="C#" Class="AuthHandler" %>

using System;
using System.Web;
//
using Crypto.EskmsAPI;
using System.IO;
using Common.Logging;

    public class AuthHandler : IHttpHandler
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(AuthHandler));

        public void ProcessRequest(HttpContext context)
        {
            
            string inputData = GetFromInputStream(context);
            if (!String.IsNullOrEmpty(inputData))
            {
                log.Debug("Request Data:" + inputData);
                byte[] responseData = DoAuthenticate(inputData);
                context.Response.OutputStream.Position = 0;
                log.Debug("start Response Out ...");
                context.Response.OutputStream.Write(responseData, 0, responseData.Length);
                log.Debug("End Response");
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
            log.Debug("Test123");
            context.Response.ContentType = "text/plain";
            context.Response.Write("Test");
            context.Response.End();
            
        }

        private static string GetFromInputStream(HttpContext context)
        {
            StreamReader sr = new StreamReader(context.Request.InputStream, System.Text.Encoding.ASCII);
            string result = sr.ReadToEnd();
            return result;
        }
        private static byte[] DoAuthenticate(string inputData)
        {
            string keyLabel = "2ICH3F0000" + inputData.Substring(0, 2) + "A";//1
            string KeyVersion = inputData.Substring(2, 2);//3
            string uid = inputData.Substring(4, 14);//17
            string enc_RanB = inputData.Substring(18, 32);//50
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
            log.Debug("Random A Start Index:" + BitConverter.ToString(iBonAuth.Output_RanB));
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