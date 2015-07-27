using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Security.Cryptography;

namespace Proxy
{
    class Program
    {
        static void Main(string[] args)
        {
            // To idendify the Smart Card CryptoGraphic Providers on your
            // computer, use the Microsoft Registry Editor (Regedit.exe).
            // The available Smart Card CryptoGraphic Providers are listed
            // in HKEY_LOCAL_MACHINE\Software\Microsoft\Cryptography\Defaults\Provider.


            // Create a new CspParameters object that identifies a 
            // Smart Card CryptoGraphic Provider.
            // The 1st parameter comes from HKEY_LOCAL_MACHINE\Software\Microsoft\Cryptography\Defaults\Provider Types.
            // The 2nd parameter comes from HKEY_LOCAL_MACHINE\Software\Microsoft\Cryptography\Defaults\Provider.
            CspParameters csp = new CspParameters(1);
            csp.KeyContainerName = "MyKeyContainer ya!!";
            csp.Flags = CspProviderFlags.UseDefaultKeyContainer;
            
            // Initialize an RSACryptoServiceProvider object using
            // the CspParameters object.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);
            Console.WriteLine("KeyName:{0}", rsa.ToXmlString(true));

            // Create some data to sign.
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            
            Console.WriteLine("Data			: " + BitConverter.ToString(data, 0, data.Length));
            
            // Sign the data using the Smart Card CryptoGraphic Provider.
            byte[] signData = rsa.SignData(data, "SHA1");

            Console.WriteLine("Signature:" + BitConverter.ToString(signData));

            bool verify = rsa.VerifyData(data, "SHA1", signData);//原始資料和sign過的資料作SHA1比對
            Console.WriteLine("驗證資料:" + verify);

            Console.ReadKey();
        }
    }
}
