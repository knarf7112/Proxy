using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//SessionKey Handler
using Crypto.ZMK.Session;

namespace Crypto.ZMK.Session_UnitTest
{
    [TestClass]
    public class UnitTest_KeyManager
    {
        private IKeyManager keyManager;

        [TestInitialize]
        public void Init()
        {
            this.keyManager = new KeyManager();

        }
        [TestMethod]
        public void TestMethod_1()
        {
            //TODO ...
            //this.keyManager.Encrypt_data()
        }

        [TestMethod]
        public void TestMethod_2()
        {

        }
        [TestCleanup]
        public void Clear()
        {

        }
    }
}
