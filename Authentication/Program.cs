﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//
using System.Security.Cryptography;
using System.Xml.Serialization;
//
using Crypto.EskmsAPI;
using System.Web.Routing;
using Newtonsoft.Json;
//
using Crypto.POCO;
//
using System.Text.RegularExpressions;

namespace Authentication
{
    //測試用專案
    public class Program
    {
        static void Main()
        {
            /******************************************************************************/
            /*將長度轉成4byte的byte[]放在byte[]陣列的頭部 用BitConvert去轉回int長度*/
            byte[] aaa = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            int length = 65538;//513;
            byte[] b1 = new byte[6];// 00000000 00000000 00000000 00000000
            b1[0] = Convert.ToByte((length << 24) >> 24);//向左位移24bit在向右24it:(只要int(4*8bytes)的第0個byte):目的是把255以上的值都位移掉(變0)
            b1[1] = Convert.ToByte((length << 16) >> 24);//向左位移16bit在向右24it:(只要int(4*8bytes)的第1個byte)目的是把255以下的值都位移掉(變0)
            b1[2] = Convert.ToByte((length << 8) >> 24);
            b1[3] = Convert.ToByte((length << 0) >> 24); 
            b1[4] = 0xFF;
            int lengthResult = BitConverter.ToInt32(b1, 0);
            byte[] ddd = aaa.Concat(aaa).ToArray();
            // Store integer 182
            int intValue = 512;
            // Convert integer 182 as a hex in a string variable
            string hexValue = intValue.ToString("X4");
            // Convert the hex string back to the number
            int intAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            /******************************************************************************/
            string checkCode = "1523ea4b7db7e072fbc4bc8bf84623fa6b10ea79";
            string checkCode2 = "QQ23ea4b7db7e072fbc4bc8bf84623fa6b10ea79";//前面兩位變QQ
            string pattern = @"[0-9A-Fa-f]{40}";//內容物為hex且連續長度為40才是sha1過的(20 bytes)
            Regex reg = new Regex(pattern);

            Console.WriteLine("checkCode Matched:" + reg.Match(checkCode).Value);

            Console.WriteLine("checkCode2 Matched:" + reg.Match(checkCode2).Value);

            Console.ReadKey();
        }

        //*************************************************************************************
        //test :if poco have attribute setting and have any exception when serializeObject? no
        public static void Main4()
        {
            string result = "";
            string err = null;
            EskmsPOCO poco = new EskmsPOCO()
            {
                Input_KeyLabel = "2ICH3F000032A",
                Input_KeyVersion = "00",
                Input_UID = "04873ABA8D2C80",
                Input_Enc_RanB = "4EF61041ABE8B0EF8B32A627B19D83AA"
            };
            poco.CheckLength(true,out err);
            result = JsonConvert.SerializeObject(poco);
            Console.WriteLine("POCO => json String:" + result);

            Console.ReadLine();
        }

        public struct Test
        {
            public String value1 { get; set; }
            public String value2 { get; set; }
            public byte[] B1 { get; set; }
            public byte[] B2 { get; set; }
        }

        public static void Main3()
        {
            Test t1 = new Test()
            {
                value1 = "qoo1",
                value2 = "test2"
            };
            Test t2 = new Test()
            {
                value1 = "T2",
                value2 = "Q2",
                B1 = new byte[] { 123, 58, 69, 77, 99, 255 },
                B2 = new byte[] { 33, 44, 55, 66, 77, 88, 99, 0 }
            };
            string jsonT1 = JsonConvert.SerializeObject(t1);
            Test t11 = JsonConvert.DeserializeObject<Test>(jsonT1);
            Console.WriteLine("t1 serialize:" + jsonT1);

            string jsonT2 = JsonConvert.SerializeObject(t2);//物件內的陣列元素會被轉換成Base64字串

            string b1 = Convert.ToBase64String(t2.B1);//
            byte[] b11 = Convert.FromBase64String(b1);
            t11 = JsonConvert.DeserializeObject<Test>(jsonT2);
            Console.WriteLine("t2 serialize:" + jsonT2);
            string filePath = @"C:\MyDirectory\My.File.bat";
            string data = System.IO.Path.GetDirectoryName(filePath);
            bool isExist = Directory.Exists(data);
            //System.Web.Routing.Route router = new System.Web.Routing.Route("http://www.google.com/",)
            //IRouteHandler r ;
        }
        //
        public static void Main2()
        {
            //iBonAuthenticate ibon = new iBonAuthenticate();
            
            int qq = 111;
            byte[] n1 = BitConverter.GetBytes(qq);
            int q2 =  BitConverter.ToInt32(n1, 0);
            string q3 = BitConverter.ToString(n1);
            Test myTest = new Test() { value1 = "Value 1", value2 = "Value 2" };
            XmlSerializer x = new XmlSerializer(myTest.GetType());
            x.Serialize(Console.Out, myTest);
            
            Console.ReadKey();
        }
        static void Main1(string[] args)
        {
            //準備資料
            string inFileName = AppDomain.CurrentDomain.BaseDirectory + "test.txt";         //要被加密的原始資料路徑
            string outFileName = AppDomain.CurrentDomain.BaseDirectory + "encTest.txt";     //加密後的檔案路徑
            string outDecFileName = AppDomain.CurrentDomain.BaseDirectory + "decTest.txt";  //解密後的檔案路徑
            Encoding fileEncoding = Encoding.ASCII;                                         //檔案編碼
            string dataOriginContent = "test123456789";                                     //檔案的內容
            byte[] decData;                                                                 //解密後的資料(byte array)
            byte[] rijnKey = Encoding.ASCII.GetBytes("0123456789abcdef");                   //加密金鑰
            byte[] rijnIV = new byte[] {                                                    //加密初始值(IV)
                                         0x00, 0x00, 0x00, 0x00, 
                                         0x00, 0x00, 0x00, 0x00,
                                         0x00, 0x00, 0x00, 0x00,
                                         0x00, 0x00, 0x00, 0x00
                                       };
            CreateFile(inFileName, dataOriginContent, fileEncoding);//建立檔案

            EncryptData(inFileName, outFileName, rijnKey, rijnIV);//產生加密檔案

            decData = DecryptData(outFileName, outDecFileName, rijnKey, rijnIV);//產生解密檔案

            Console.WriteLine("解密檔案內容:{0}", fileEncoding.GetString(decData, 0, decData.Length));
            Console.ReadKey();
        }

        //建立檔案
        static void CreateFile(string fileName,string content,Encoding encoding)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] contentBytes = encoding.GetBytes(content);
                fs.Write(contentBytes, 0, contentBytes.Length);
                fs.Flush();
            }
        }
        /// <summary>
        /// 使用rijn演算法加密資料
        /// ref:https://msdn.microsoft.com/zh-tw/library/system.security.cryptography.symmetricalgorithm(v=vs.110).aspx
        /// </summary>
        /// <param name="inName">要加密的檔案路徑</param>
        /// <param name="outName">加密後新產生的檔案路徑</param>
        /// <param name="rijnKey">加密金鑰(規定要16 byte)</param>
        /// <param name="rijnIV">initial value初始值(加解密雙方都要一樣)</param>
        static void EncryptData(string inName,string outName,byte[] rijnKey,byte[] rijnIV)
        {
            //Create the file streams to handle the input and output files.
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);

            //Create variables to help with read and write.
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
            long rdlen = 0;             //This is the total number of bytes written.
            long total_len = fin.Length;//This is the total length of the input file.
            int len;                    //This is the number of bytes to be written at a time.

            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();//Creates the default implementation, which is RijndaelManaged.
            CryptoStream encStream = new CryptoStream(fout, rijn.CreateEncryptor(rijnKey, rijnIV), CryptoStreamMode.Write);//使用rijn演算法來加密後寫入CryptoStream

            Console.WriteLine("Encrypting...");

            //Read from the input file, then encrypt and write to the output file.
            while (rdlen < total_len)
            {
                len = fin.Read(bin, 0, bin.Length);
                encStream.Write(bin, 0, len);
                rdlen = rdlen + len;
                Console.WriteLine("{0}bytes processed", rdlen);
            }

            encStream.Close();
            fout.Close();
            fin.Close();
        }

        /// <summary>
        /// 同上反推解密
        /// </summary>
        /// <param name="inName">要解密的檔案路徑</param>
        /// <param name="outName">解密成原始資料後的檔案路徑</param>
        /// <param name="rijnKey">加密金鑰(規定要16 byte)</param>
        /// <param name="rijnIV">initial value初始值(加解密雙方都要一樣)</param>
        /// <returns></returns>
        static byte[] DecryptData(string inName, string outName, byte[] rijnKey, byte[] rijnIV)
        {
            //Create the file streams to handle the input and output files.
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);

            byte[] decData = new byte[0x1000];  //用來裝解密後的資料
            int decData_len = 0;                //解密後資料的長度

            fin.Position = 0;                   //設定資料流讀取的初始位置

            //data_len = fin.Read(data_Buffer, 0, data_Buffer.Length);//(這是錯誤的)不能先讀,讀完資料流內就沒資料了
            
            //使用rijn演算法
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            //使用cryptoStream來讀取加密的資料流並設定解密的cryptor(Key,IV)
            CryptoStream decStream = new CryptoStream(fin, rijn.CreateDecryptor(rijnKey, rijnIV), CryptoStreamMode.Read);
            
            Console.WriteLine("Decrypting...");
            //*******************************************************
            //StreamReader streamReader = new StreamReader(decStream);
            //string data = streamReader.ReadToEnd();
            //Console.WriteLine("Decrypt data:{0}", data);
            //decData = Encoding.ASCII.GetBytes(data);
            //*******************************************************
            decData_len = decStream.Read(decData, 0, decData.Length);//將解密的資料讀到buffer
            if (!decStream.HasFlushedFinalBlock)
                decStream.FlushFinalBlock();
            Array.Resize(ref decData,decData_len);//縮一下buffer大小
            fout.Write(decData, 0, decData.Length);//從另一個資料流將解密後的資料寫入
            fout.Flush();


            decStream.Clear();
            decStream.Close();
            fin.Close();
            fout.Close();

            return decData;
        }
    }
}
