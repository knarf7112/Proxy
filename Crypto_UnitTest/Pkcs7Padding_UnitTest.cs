using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using Crypto;
using System.Diagnostics;

namespace Crypto_UnitTest
{
    [TestClass]
    public class Pkcs7Padding_UnitTest
    {
        private IHexConverter hexConverter;
        private IPaddingHelper pkcs7PaddingHelper;
        private IByteWorker byteWorker;

        [TestInitialize]
        public void InitContext()
        {
            this.hexConverter = new HexConverter();
            this.pkcs7PaddingHelper = new Pkcs7PaddingHelper();
            this.byteWorker = new ByteWorker();
        }

        [TestMethod]
        public void Test_AddPadding()
        {
            byte[] data16 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            for (int i = 0; i <= data16.Length; i++)
            {
                Debug.WriteLine
                (
                    this.pkcs7PaddingHelper.AddPadding
                    (
                        this.byteWorker.Combine
                        (
                            SymCryptor.ConstZero
                          , this.byteWorker.SubArray(data16, 0, i)
                        )
                    )
                );
            }
        }
    }
}
