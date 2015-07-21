using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto;

namespace Crypto_UnitTest
{
    [TestClass]
    public class SymCryptor_UnitTest
    {
        private ISymCryptor symCryptor;

        public void Init()
        {
            this.symCryptor = new SymCryptor("AES");
        }

        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
