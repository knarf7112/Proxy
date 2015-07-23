using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using Crypto;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.IO;

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
            int expect = data16.Length;
            for (int i = 0; i < data16.Length; i++)
            {
                byte[] subArr = new byte[i + 1];
                Array.Copy(data16, subArr, i + 1);
                byte[] result = this.pkcs7PaddingHelper.AddPadding(subArr);
                Debug.WriteLine("Padding後(Byte Array): \t" + ArrToString(result));
                if (i == (data16.Length - 1))
                    expect = data16.Length + (i + 1); //沒有剩餘資料要多補一個Block區塊
                Assert.AreEqual(expect, result.Length);
            }
        }
        [TestMethod]
        public void Test_RemovePadding()
        {
            byte[] data16 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            for (int i = 0; i < data16.Length; i++)
            {
                int expect = i + 1;
                //*************************************
                //先Padding資料
                byte[] subArr = new byte[i + 1];
                Array.Copy(data16, subArr, i + 1);
                byte[] padArr = this.pkcs7PaddingHelper.AddPadding(subArr);
                //Debug.WriteLine("Padding後(Byte Array): \t" + ArrToString(padArr));
                //**************************************
                //再移除Padding資料
                subArr = this.pkcs7PaddingHelper.RemovePadding(padArr);
                Debug.WriteLine("移除Padding(Byte Array): \t" + ArrToString(subArr));
                Assert.AreEqual(expect, subArr.Length);
            }
        }

        [TestMethod]
        public void Test_AddPaddingFile()
        {
            Assembly assembly = Assembly.Load("Crypto_UnitTest");//載入測試組件
            string paddingFileFullPath = AppDomain.CurrentDomain.BaseDirectory + @"\padding.dat";
            //讀取測試組件內的內嵌檔案
            using (Stream sourceStream = assembly.GetManifestResourceStream("Crypto_UnitTest.TestFile.dec_BRQA_567_20141225_B06B_01"))
            {
                using (FileStream destanationStream = new FileStream(paddingFileFullPath, FileMode.Create, FileAccess.Write))
                {
                    this.pkcs7PaddingHelper.AddPadding(sourceStream, destanationStream);
                    Debug.WriteLine("Padding前檔案長度: " + sourceStream.Length);
                    Debug.WriteLine("Padding後檔案長度: " + destanationStream.Length);
                    Debug.WriteLine("是否整除BlockSize: " + ((destanationStream.Length % this.pkcs7PaddingHelper.BlockSize) == 0).ToString());
                }
            }
        }

        [TestMethod]
        public void Test_RemovePaddingFile()
        {
            Assembly assembly = Assembly.Load("Crypto_UnitTest");//載入測試組件
            string paddingFileFullPath = AppDomain.CurrentDomain.BaseDirectory + @"\padding.dat";
            string paddingRemoveFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\paddingRemove.dat";
            long origin_FileLength = 0;
            //**************************************************************************
            //1.先Padding原始檔案產生新檔
            //讀取測試組件內的內嵌檔案
            using (Stream sourceStream = assembly.GetManifestResourceStream("Crypto_UnitTest.TestFile.dec_BRQA_567_20141225_B06B_01"))
            {
                origin_FileLength = sourceStream.Length;
                using (FileStream destanationStream = new FileStream(paddingFileFullPath, FileMode.Create, FileAccess.Write))
                {
                    this.pkcs7PaddingHelper.AddPadding(sourceStream, destanationStream);
                }
            }
            //***************************************************************************
            //2.再將Padding過的檔移除Padding後寫入新檔
            using (FileStream padFileStream = new FileStream(paddingFileFullPath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream removePaddingStream = new FileStream(paddingRemoveFilePath, FileMode.Create, FileAccess.Write))
                {
                    this.pkcs7PaddingHelper.RemovePadding(padFileStream, removePaddingStream);
                    Debug.WriteLine("移除Padding前檔案長度: " + padFileStream.Length);
                    Debug.WriteLine("移除Padding後檔案長度: " + removePaddingStream.Length);
                    Debug.WriteLine("是否與原始檔案長度相同: " + (removePaddingStream.Length == origin_FileLength).ToString());
                }
            }
        }

        //extention
        static string ArrToString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in arr)
            {
                sb.Append(string.Format("{0} \t",b));
            }
            return sb.ToString().Trim();
        }
    }
}
