using System;
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

namespace Authentication
{
    public class Program
    {
        public class Test
        {
            public String value1 { get; set; }
            public String value2 { get; set; }
        }

        public static void Main()
        {
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
