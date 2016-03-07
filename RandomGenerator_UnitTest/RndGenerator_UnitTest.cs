using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//project
using RandomGenerator;
//file check
using System.IO;
//process
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
namespace Radom_Utility_UnitTest
{
    [TestClass]
    public class RndGenerator_UnitTest
    {
        private IRndGenerator GenRnd;

        [TestInitialize]
        public void Init()
        {
            this.GenRnd = new RndGenerator();
        }

        [TestMethod]
        public void TestMethod_WriteFile()
        {
            string fileName = "Random.txt";
            RndGenerator.WriteFile(4096,fileName);
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\" + fileName;
            Assert.IsTrue(File.Exists(filePath));
            //喵一下檔案
            Process.Start(filePath);
        }

        [TestMethod]
        public void TestMethod_GetMaxIndex()
        {
            int expectedMaxIndex = 4080;
            int actualMaxIndex = this.GenRnd.GetMaxIndex();
            Assert.AreEqual(expectedMaxIndex, actualMaxIndex);
            Debug.WriteLine("expected:{0}, actual:{1}", expectedMaxIndex, actualMaxIndex);
        }

        [TestMethod]
        public void TestMethod_Get_Random()
        {
            IDictionary<int, int> countDic = new Dictionary<int, int>();
            
            int notExpected = -1;
            int count = this.GenRnd.GetMaxIndex();
            for (int i = 0; i < count; i++)
            {
                int index = -1;
                byte[] result = this.GenRnd.Get_Random(out index);
                Assert.IsNotNull(result);
                Assert.AreNotEqual(notExpected,index);
                Debug.WriteLine("index:{0}  byte[]:{1}", index.ToString("D4"), BitConverter.ToString(result).Replace("-", ""));
                if (countDic.ContainsKey(index))
                {
                    countDic[index]++; 
                }
                else
                {
                    countDic.Add(index, 0);
                }
            }
            //repeat count > 1(看數據分布狀況)
            foreach (var key in countDic.Keys)
            {
                if (countDic[key] > 0)
                    Debug.WriteLine("index:{0} Repeat Count:{1}", key, countDic[key]);
            }
        }

        [TestMethod]
        public void TestMethod_Get_RandomFromIndex()
        {
            int maxIndex = this.GenRnd.GetMaxIndex();
            int index1_outOfRange = -1;
            int index2_outOfRange = maxIndex + 1;
            for (int i = 0; i < maxIndex; i++)
            {
                byte[] result = this.GenRnd.Get_RandomFromIndex(i);
                Assert.IsNotNull(result);
                Debug.WriteLine("index:{0} byte[]:{1}", i, BitConverter.ToString(result).Replace("-", ""));
            }
            ArgumentOutOfRangeException ex1 = null;
            ArgumentOutOfRangeException ex2 = null;
            //out of range 
            try
            {
                byte[] result = this.GenRnd.Get_RandomFromIndex(index1_outOfRange);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ex1 = ex;
            }
            Assert.IsNotNull(ex1, "索引低於最小範圍需拋異常");
            try
            {
                byte[] result = this.GenRnd.Get_RandomFromIndex(index2_outOfRange);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ex2 = ex;
            }
            Assert.IsNotNull(ex2, "索引超出最大範圍需拋異常");
        }

        [TestCleanup]
        public void Clean()
        {

        }
    }
}
