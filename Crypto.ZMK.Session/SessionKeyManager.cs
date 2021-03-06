﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//SymCryptor Object
using Crypto;
//Random List
using RandomGenerator;
using System.Diagnostics;

namespace Crypto.ZMK.Session
{
    /// <summary>
    /// SessionKey Handler
    /// </summary>
    public class SessionKeyManager : IKeyManager, IDisposable
    {
        private ISymCryptor symCryptor;

        private IRndGenerator RndList;

        public SessionKeyManager()
        {
            this.symCryptor = new SymCryptor("AES");
            this.RndList = new RndGenerator();
            
        }

        /// <summary>
        /// 若有輸入DiverseKey則產生加密的DiverseKey與IV索引値(int)
        /// 若無輸入DiverseKey則產生加密的SessionKey與RndA索引値(int)
        /// </summary>
        /// <param name="key">Key:16 bytes </param>
        /// <param name="rndIndex">亂數表索引</param>
        /// <param name="enc_data">加密後的數據:16 bytes</param>
        /// <param name="diverseKey">MasterKey:16 bytes</param>
        public virtual void Encrypt_data(byte[] key,out int rndIndex,out byte[] enc_data,byte[] diverseKey = null)
        {
            //隨機輸出一組16 bytes random陣列與其位置索引値
            byte[] data = this.RndList.Get_Random(out rndIndex);
            this.symCryptor.SetKey(key);
            if (diverseKey != null)
            {
                //SessionKey(Key) + Ran(IV) + DiverseKey(Data) => Encrypted_DiverseKey
                this.symCryptor.SetIV(data);
                enc_data = this.symCryptor.Encrypt(diverseKey);
            }
            else
            {
                //ZMK_DATA(Key) + 0(IV) + RnA(Data) => SessionKey
                this.symCryptor.SetIV(SymCryptor.ConstZero);
                enc_data = this.symCryptor.Encrypt(data);
            }
        }

        /// <summary>
        /// 依索引値與ZMK_data取得SessionKey
        /// </summary>
        /// <param name="key">ZNK_data</param>
        /// <param name="rndIndex">random list index</param>
        /// <returns>SessionKey:16 bytes</returns>
        public virtual byte[] Get_SessionKey(byte[] key, int rndIndex)
        {
            byte[] enc_data = null;
            //隨機輸出一組16 bytes random陣列與其位置索引値
            byte[] data = this.RndList.Get_RandomFromIndex(rndIndex);
            this.symCryptor.SetKey(key);
            this.symCryptor.SetIV(SymCryptor.ConstZero);
            //SessionKey(Key) + Ran(IV) + DiverseKey(Data) => Encrypted_DiverseKey
            enc_data = this.symCryptor.Encrypt(data);
            return enc_data;
        }

        /// <summary>
        /// 依索引取得隨機陣列的物件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual byte[] Get_RanDom_data(int index)
        {
            return this.RndList.Get_RandomFromIndex(index);
        }

        /// <summary>
        /// 作AES加密資料,設定Key與依據指定索引從隨機表取得陣列並設定iv
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="rndIndex">隨機物件列表的索引</param>
        /// <param name="data">要加密的</param>
        /// <returns>加密後的數據</returns>
        public virtual byte[] Encrypt_data(byte[] key, int rndIndex, byte[] data)
        {
            byte[] enc_data = null;
            //隨機輸出一組16 bytes random陣列與其位置索引値
            byte[] iv = this.RndList.Get_RandomFromIndex(rndIndex);
            this.symCryptor.SetKey(key);
            this.symCryptor.SetIV(iv);
            //SessionKey(Key) + Ran(IV) + DiverseKey(Data) => Encrypted_DiverseKey
            enc_data = this.symCryptor.Encrypt(data);
            return enc_data;
        }

        /// <summary>
        /// 從隨機列表內取得一組隨機索引
        /// </summary>
        /// <returns>random index</returns>
        public virtual int Get_RandomIndex()
        {
            int rndIndex = -1;
            this.RndList.Get_Random(out rndIndex);
            return rndIndex;
        }

        /// <summary>
        /// 若有輸入亂數表索引値則解密EncDiverseKey
        /// 若無輸入亂數表索引値則還原SessionKey的原始Data(即亂數表的指定索引陣列:16 bytes)
        /// </summary>
        /// <param name="key">SessionKey/ZMK_DATA</param>
        /// <param name="enc_data">EncDiverseKey/SessionKey</param>
        /// <param name="rndIndex">Ran(IV)/0(IV)</param>
        /// <returns>DiverseKey/RnA</returns>
        public virtual byte[] Decrypt_data(byte[] key, byte[] enc_data, int rndIndex = -1)
        {
            this.symCryptor.SetKey(key);
            if (rndIndex.Equals(-1))
            {
                //ZMK_DATA(Key) + 0(IV) + SessionKey(Data) => RnA
                this.symCryptor.SetIV(SymCryptor.ConstZero);
                return this.symCryptor.Decrypt(enc_data);
            }
            else
            {
                //SessionKey(Key) + Ran(IV) + EncDiverseKey(Data) => DiverseKey
                byte[] data = this.RndList.Get_RandomFromIndex(rndIndex);
                this.symCryptor.SetIV(data);
                return this.symCryptor.Decrypt(enc_data);
            }
        }

        /// <summary>
        /// 取得SessionKey的A Part索引値與B Part陣列:16 bytes
        /// </summary>
        /// <param name="sessionKey">針對每間特約機構產生的傳輸加密金鑰</param>
        /// <param name="A_part_Index">A Part索引値</param>
        /// <param name="B_part">B Part陣列:16 bytes</param>
        /// <returns>true:建立AB Part成功/false:異常</returns>
        public virtual bool Gen_AB_Part(byte[] sessionKey, out int A_part_Index, out byte[] B_part)
        {
            try
            {
                byte[] data = this.RndList.Get_Random(out A_part_Index);
                B_part = this.Get_XOR_data(sessionKey, data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(this.GetType().Name + "[Get_SessionKey] Error:" + ex.Message);
                B_part = null;
                A_part_Index = -1;
                return false;
            }
        }

        /// <summary>
        /// 取得SessionKey從輸入的A Part索引値與B Part陣列:16 bytes
        /// </summary>
        /// <param name="A_part_Index">A Part索引値</param>
        /// <param name="B_part">B Part陣列:16 bytes</param>
        /// <param name="sessionKey">SessionKey:16 bytes</param>
        /// <returns>true:取得SessionKey成功/false:異常</returns>
        public virtual bool Get_SessionKey(int A_part_Index, byte[] B_part,out byte[] sessionKey)
        {
            try
            {
                byte[] data = this.RndList.Get_RandomFromIndex(A_part_Index);
                sessionKey = this.Get_XOR_data(data, B_part);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(this.GetType().Name + "[Get_SessionKey] Error:" + ex.Message);
                sessionKey = null;
                return false;
            }
        }

        /// <summary>
        /// Get Xor data when input data1 and data2
        /// </summary>
        /// <param name="data1">data1:16 bytes</param>
        /// <param name="data2">data2:16 bytes</param>
        /// <returns>Xor data:16 bytes</returns>
        protected virtual byte[] Get_XOR_data(byte[] data1, byte[] data2)
        {
            if (data1.Length != data2.Length)
                throw new ArgumentException("data1 length must be equals data2 length");
            byte[] result = new byte[data1.Length];
            for (int i = 0; i < data1.Length; i++)
            {
                result[i] = (byte)(data1[i] ^ data2[i]);
            }

            return result;
        }

        public void Dispose()
        {
            this.symCryptor = null;
            this.RndList = null;
        }
    }
}
