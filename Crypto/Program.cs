using System;
using System.Text;
//
using System.Security.Cryptography;

namespace Crypto
{
    //直接執行測試物件用
    class Program
    {
        static void Main(string[] args)
        {
            string key = "0123456789ABCDEF";//KEY一定要16 bytes以上
            string data = "test123456789";//資料一定要Padding
            byte[] encData;
            byte[] decData;
            ISymCryptor symCryptor = new SymCryptor();
            symCryptor.SetAlgorithm("AES",CipherMode.CBC,PaddingMode.PKCS7);//自動會padding
            symCryptor.SetIV(SymCryptor.ConstZero);
            symCryptor.SetKey(Encoding.ASCII.GetBytes(key));

            encData = symCryptor.Encrypt(Encoding.ASCII.GetBytes(data));
            decData = symCryptor.Decrypt(encData);

            Console.WriteLine("原始資料:{0}",data);
            Console.WriteLine("加密後byteArray:{0}", toString(encData));
            Console.WriteLine("加密後資料:{0}", Encoding.ASCII.GetString(encData));//toString(encData));
            Console.WriteLine("解密後資料:{0}", Encoding.ASCII.GetString(decData));

            Console.ReadKey();
        }

        static string toString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data)
            {
                sb.Append(string.Format("{0:X2} ", b));
            }

            return sb.ToString().Trim();
        }
    }
}
