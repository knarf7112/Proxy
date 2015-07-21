using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.IO;

namespace Crypto
{
    /// <summary>
    /// 遵照Pkcs7規範將資料陣列尾部補齊規定的BlockSize大小
    /// </summary>
    public class Pkcs7PaddingHelper : IPaddingHelper
    {
        /// <summary>
        /// 每個區塊的大小,padding前檢查每個block大小用
        /// </summary>
        public int BlockSize { get; set; }
        public int BufferSize { get; set; }

        #region Constructor
        public Pkcs7PaddingHelper()
        {
            this.BlockSize = 16;
            this.BufferSize = 4096;
        }
        #endregion
        /// <summary>
        /// Padding data source
        /// ex: {1, 2, 3} ==Pkcs7==> { 1,2,3,13,13,13,13,13,13,13,13,13,13,13,13,13}
        /// </summary>
        /// <param name="src">來源資料</param>
        /// <returns>Padding後的資料陣列</returns>
        public byte[] AddPadding(byte[] src)
        {
            //檢查資料來源與blockSize的餘數,用來看最後要補齊多少差距
            int last = src.Length % this.BlockSize;//若整除則補一整個BlockSize,即補{16,16,16...}
            int left = this.BlockSize - last;//最後一個區塊與BlockSize的差距

            byte[] combineArr = new byte[src.Length + left];
            //將原始資料複製到新陣列的前半部
            Buffer.BlockCopy(src, 0, combineArr, 0, src.Length);

            //補上後半部Padding的數據,遵照#Pkcs7,將不足的補齊,
            //假設資料(byte)為{ 1, 2, 3 } BlockSize為16所以不足13個資料,
            //所以補足到16個=>{ 1,2,3,13,13,13,13,13,13,13,13,13,13,13,13,13}
            for (int i = src.Length; i < combineArr.Length; i++)
            {
                combineArr[i] = (byte)left;
            }
            return combineArr;
        }
        /// <summary>
        /// 將來源資料流資內的資料和Padding陣列寫入目的資料流
        /// </summary>
        /// <param name="src">來源資料流</param>
        /// <param name="dest">目的資料流</param>
        public void AddPadding(Stream src, Stream dest)
        {
            int last = (int)(src.Length % this.BlockSize);
            int left = this.BlockSize - last;

            byte[] buffer = new byte[this.BlockSize];   //用來暫存讀取的資料
            int readCnt = 0;                            //用來確定讀到的資料長度
            //若來源資料流還讀的到資料,就將資料讀到buffer
            while ((readCnt = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                //將讀到的資料(buffer)寫入目的資料流
                dest.Write(buffer, 0, readCnt);
            }

            //產生尾部的padding陣列 
            //ex: 
            //   left:7 ==產生7個元素==> {7,7,7,7,7,7,7}
            byte[] padArr = new byte[left];
            for (int i = 0; i < left; i++)
            {
                padArr[i] = (byte)left;
            }

            //將padding後的Byte Array寫入目的資料流
            dest.Write(padArr, 0, left);
        }

        /// <summary>
        /// 移除委部Padding過的資料
        /// ex:{ 1,2,3,13,13,13,13,13,13,13,13,13,13,13,13,13} ==Pkcs7==> {1, 2, 3}
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public byte[] RemovePadding(byte[] src)
        {
            int left = src[src.Length - 1];//直接看陣列最後一個資料,值即padding數量
            //若最後一個Padding元素大於0且小於BlockSize
            if (left > 0 && left < this.BlockSize)
            {
                byte[] unPaddingArr = new byte[src.Length - left];
                for (int i = 0; i < unPaddingArr.Length; i++)
                {
                    unPaddingArr[i] = src[i];
                }

                return unPaddingArr;
            }
            else
            {
                // no padding
                return src;
            }
        }

        /// <summary>
        /// 移除Padding的陣列資料流
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public void RemovePadding(Stream src, Stream dest)
        {
            byte[] buffer = new byte[this.BlockSize];//暫存
            int readCnt = 0;
            //ref:https://msdn.microsoft.com/zh-tw/library/system.io.stream.seek(v=vs.110).aspx
            src.Seek(-1, SeekOrigin.End);//指標直接指向最後一個位置的前一個
            //src.Position = src.Length - 2;//這沒試過 應該一樣會指向最後一個位置
            int left = src.ReadByte();//資料流往前推進並回傳此位置的值,取得padding值
            long realSize = src.Length - left;//去掉Padding的資料長度(真實的資料長度)

            //int loopCnt = (int)(realSize / buffer.Length);//要跑的迴圈數
            //int lastLeft = (int)(realSize % buffer.Length);//最後剩餘的資料長度

            if (left > 0 && left <= this.BlockSize)
            {
                //ref:http://stackoverflow.com/questions/7238929/stream-seek0-seekorigin-begin-or-position-0
                src.Seek(0, SeekOrigin.Begin);//指標設定來源資料流的第一個位置當起始位置,seek看的是相對位置
                dest.Position = 0;//position看的是絕對位置
                //從來源資料流個別讀取一個Block資料放在buffer並寫到目的資料流
                while ((readCnt = src.Read(buffer, 0, buffer.Length)) > 0)
                {
                    dest.Write(buffer, 0, readCnt);
                }

                //***********************************************
                //Dinnes版
                //先塞完整Block的資料到目的資料流
                //for (int i = 0; i < loopCnt; i++)
                //{
                //    readCnt = src.Read(buffer, 0, buffer.Length);
                //    dest.Write(buffer, 0, readCnt);
                //}
                ////再塞剩餘的部分到資料到目的資料流
                //if (lastLeft > 0)
                //{
                //    readCnt = src.Read(buffer, 0, lastLeft);
                //    dest.Write(buffer, 0, readCnt);
                //}
                //***********************************************
            }
        }
    }
}
