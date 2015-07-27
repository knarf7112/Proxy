using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Security.Cryptography;

namespace Crypto.CommonUtility
{
    public class RandomWorker : IRandWorker
    {

        private RandomNumberGenerator ranGen;

        public RandomWorker()
        {
            this.ranGen = new RNGCryptoServiceProvider();
        }


        public byte[] GetBytes(int size)
        {
            byte[] random = new byte[size];
            this.ranGen.GetBytes(random);//call by ref
            return random;
        }

        public void GetBytes(byte[] bytes)
        {
            this.ranGen.GetBytes(bytes);
        }
    }
}
