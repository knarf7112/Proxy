using Crypto.CommonUtility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// NIST Special Publication 800-38B 
    /// Recommendation for Block Cipher Modes of Operation: 
    /// The CMAC(Cipher-based Message Authentication Code) Mode for Authentication
    /// http://csrc.nist.gov/publications/nistpubs/800-38B/SP_800-38B.pdf
    /// </summary>
    public class AesCMac2Worker : ICMac2Worker
    {
        //
        public static readonly int ConstBlockSize = 16;
        public static readonly byte FirstPadding = 0x80; // 0b10000000
        public static readonly byte[] ConstZero = new byte[]
        { 
            0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0 
        };
        public static readonly byte[] ConstRb = new byte[]
        { 
            0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0
          , 0, 0, 0, 0x87 
        };
        //public static const uint CKM_AES_ECB = 0x1081;
        //public static const uint CKM_AES_CBC = 0x1082;
        //
        public IByteWorker ByteWorker { private get; set; }
        public IBytesBitwiser BytesBitwiser { private get; set; }
        public HexConverter HexConverter { private get; set; }
        public IEsKmsWebApi EsCryptor { get; set; }
        //
        //private string macKey;        
        private int macLength = ConstBlockSize;
        private byte[] iv = ConstZero;
        private byte[] dataInput = null;
        private byte[] k1 = null;
        private byte[] k2 = null;
        //
        //private secret_key_t key = new secret_key_t();
        //private mechanism_param_t mechanism = new mechanism_param_t();
        private string keyLabel = null;

        #region Constructor

        public AesCMac2Worker()
        {
            this.ByteWorker = new ByteWorker();
            this.BytesBitwiser = new BytesBitwiser();
            this.HexConverter = new HexConverter();
            this.EsCryptor = new EsKmsWebApi()
            {
                Url = "http://127.0.0.1:8081/eGATEsKMS/interface",//"http://10.27.68.163:8080/eGATEsKMS/interface",
                AppCode = "APP_001",
                AuthCode = "12345678",
                AppName = "icash2Test",
                HttpMethod = "POST",
                HexConverter = new HexConverter(),
                HashWorker = new HashWorker()
                {
                    HashAlg = "SHA1",
                    HexConverter = new HexConverter()
                }
            };

        }

        public AesCMac2Worker(IEsKmsWebApi esKmsWebApi)
        {
            this.EsCryptor = esKmsWebApi;
            this.ByteWorker = new ByteWorker();
            this.BytesBitwiser = new BytesBitwiser();
            this.HexConverter = new HexConverter();
        }
        #endregion

        public void DataInput(byte[] m)
        {
            this.dataInput = new byte[m.Length];
            Array.Copy(m, this.dataInput, m.Length);
        }

        public void SetMacKey(string keyLabel)
        {
            this.keyLabel = keyLabel;
            //this.key.key_label = keyLabel;
            //this.key.key_type = (int)KMS_KEY_TYPE_CODE.KMS_KEY_TYPE_KEY;
            //this.key.slot = 0;
            //
            this.k1 = this.k2 = null;
        }

        public void SetMacLength(int macLength)
        {
            if (macLength > ConstBlockSize)
            {
                throw new ArgumentOutOfRangeException("Max MAC size: " + ConstBlockSize);
            }
            this.macLength = macLength;
        }

        public void SetIv(byte[] iv)
        {
            this.iv = new byte[iv.Length];
            Array.Copy(iv, this.iv, iv.Length);
            this.k1 = this.k2 = null;
        }
        /*
        +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        +                    Algorithm Generate_Subkey                      +
        +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        +                                                                   +
        +   Input    : K (128-bit key)                                      +
        +   Output   : K1 (128-bit first subkey)                            +
        +              K2 (128-bit second subkey)                           +
        +-------------------------------------------------------------------+
        +                                                                   +
        +   Constants: const_Zero is 0x00000000000000000000000000000000     +
        +              const_Rb   is 0x00000000000000000000000000000087     +
        +   Variables: K0         for output of AES-128 applied to 0^128    +
        +                                                                   +
        +   Step 1.  K0 := AES-128(K, const_Zero);                           +
        +   Step 2.  if MSB(K0) is equal to 0                                +
        +            then    K1 := K0 << 1;                                  +
        +            else    K1 := (K0 << 1) XOR const_Rb;                   +
        +   Step 3.  if MSB(K1) is equal to 0                               +
        +            then    K2 := K1 << 1;                                 +
        +            else    K2 := (K1 << 1) XOR const_Rb;                  +
        +   Step 4.  return K1, K2;                                         +
        +                                                                   +
        +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        */
        private void getSubKeys()
        {
            byte[] k0 = null;
            // 
            //mechanism_param_t mechanism = new mechanism_param_t();
            //data_blob_t data_in = new data_blob_t();
            //data_blob_t data_out = new data_blob_t();
            //result_t result = new result_t();
            //

            byte[] data = this.ByteWorker.Fill(16, 0x00);//第一段取k0使用4225,BlobValue="0000000000000000"
                //byte[] out_data_buf = new byte[16];
            //var hd1 = GCHandle.Alloc(data, GCHandleType.Pinned);
            //var hd2 = GCHandle.Alloc(out_data_buf, GCHandleType.Pinned);
            try
            {
                //mechanism.mechanism = EsKmsApi.CKM_AES_ECB;
                ////
                //data_in.name = null;
                //data_in.type = (int)(KMS_DATA_TYPE_CODE.KMS_DATA_BLOB_TYPE_BINARY);
                //data_in.value = hd1.AddrOfPinnedObject();
                //data_in.value_len = data.Length;
                ////設定密碼運算的結果存放處
                //data_out.value = hd2.AddrOfPinnedObject();
                //data_out.value_len = out_data_buf.Length;

                //密碼運算                    
                //this.EsCryptor.Cipher(this.key, (uint)KMS_CIPHER_CODE.KMS_CIPHER_METHOD_ENCRYPT, mechanism, data_in, ref data_out, ref result);
                this.EsCryptor.CipherMode = "ECB";
                k0 = this.EsCryptor.Encrypt(this.keyLabel, null, data);
                Debug.WriteLine("k0 == null:" + (k0 == null).ToString());
                //k0 = this.ByteWorker.SubArray(out_data_buf, 0, data_out.value_len);
                //log.Debug("K0:[" + this.HexConverter.Bytes2Hex(k0) + "]");
            }
            catch (Exception ex)
            {
                //log.Error(ex.StackTrace);
                throw new Exception("K0 build fail:" + ex.Message);
            }
            //finally
            //{
            //    //hd1.Free();
            //    //hd2.Free();
            //}
            //
            this.k1 = this.getNextSubKey(k0);
            //log.Debug("K1:[" + this.HexConverter.Bytes2Hex(this.k1) + "]");
            //
            this.k2 = this.getNextSubKey(k1);
            //log.Debug("K2:[" + this.HexConverter.Bytes2Hex(this.k2) + "]");
        }

        private byte[] getNextSubKey(byte[] kx)
        {
            byte[] k;
            if (!this.BytesBitwiser.MsbOne(kx))
            {
                k = this.BytesBitwiser.ShiftLeft(kx, 1);
            }
            else
            {
                k = this.BytesBitwiser.ExclusiveOr(this.BytesBitwiser.ShiftLeft(kx, 1), ConstRb);
            }
            return k;
        }

        /*
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +                   Algorithm AES-CMAC                              +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +                                                                   +
   +   Input    : K    ( 128-bit key )                                 +
   +            : M    ( message to be authenticated )                 +
   +            : len  ( length of the message in octets )             +
   +   Output   : T    ( message authentication code )                 +
   +                                                                   +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
   +   Constants: const_Zero is 0x00000000000000000000000000000000     +
   +              const_Bsize is 16                                    +
   +                                                                   +
   +   Variables: K1, K2 for 128-bit subkeys                           +
   +              M_i is the i-th block (i=1..ceil(len/const_Bsize))   +
   +              M_last is the last block xor-ed with K1 or K2        +
   +              n      for number of blocks to be processed          +
   +              r      for number of octets of last block            +
   +         needPadding for denoting if last block is complete or not +
   +                                                                   +
   +   Step 1.  (K1,K2) := Generate_Subkey(K);                         +
   +   Step 2.  n := ceil(len/const_Bsize);                            +
   +   Step 3.  if n = 0                                               +
   +            then                                                   +
   +                 n := 1;                                           +
   +                 needPadding := true;                              +
   +            else                                                   +
   +                 if len mod const_Bsize is 0                       +
   +                 then needPadding := false;                        +
   +                 else needPadding := true;                         +
   +                                                                   +
   +   Step 4.  if needPadding is false                                +
   +            then M_last := M_n XOR K1;                             +
   +            else M_last := padding(M_n) XOR K2;                    +
   +   Step 5.  X := const_Zero;                                       +
   +   Step 6.  for i := 1 to n-1 do                                   +
   +                begin                                              +
   +                  Y := X XOR M_i;                                  +
   +                  X := AES-128(K,Y);                               +
   +                end                                                +
   +            Y := M_last XOR X;                                     +
   +            T := AES-128(K,Y);                                     +
   +   Step 7.  return T;                                              +
   +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
         *///inputData="01+uid+ICASH+uid+ICASH+uid"
        private byte[] getFullMac(byte[] inputData)
        {
            if (inputData == null)
            {
                Debug.WriteLine("[getFullMac]inputData is null");
            }
            //this.EsCryptor.Authentication();
            if (this.k1 == null)
            {
                this.getSubKeys();
            }
            bool needPadding = false;
            int nBlock = inputData.Length / ConstBlockSize;
            if (nBlock == 0)
            {
                nBlock = 1;
                needPadding = true;
            }
            else
            {
                if (inputData.Length % ConstBlockSize == 0)
                {
                    needPadding = false;
                }
                else
                {
                    needPadding = true;
                    nBlock += 1;
                }
            }
            //
            byte[] mLast; // = new byte[ConstBlockSize];
            if (!needPadding)
            {
                //Array.Copy(inputData, inputData.Length - ConstBlockSize, mLast, 0, ConstBlockSize);
                mLast = this.BytesBitwiser.ExclusiveOr
                (
                     this.ByteWorker.SubArray(inputData, inputData.Length - ConstBlockSize, ConstBlockSize)
                   , this.k1
                );
            }
            else
            {
                int lastBlockSize = inputData.Length % ConstBlockSize;
                mLast = this.ByteWorker.CMacPadding
                (
                    this.ByteWorker.SubArray(inputData, inputData.Length - lastBlockSize, lastBlockSize)
                );
                //log.Debug("last block:" + this.HexConverter.Bytes2Hex(mLast));
                // do xor with k2
                mLast = this.BytesBitwiser.ExclusiveOr(mLast, this.k2);
                //log.Debug("last block xor k2:" + this.HexConverter.Bytes2Hex(mLast));
            }
            //
            byte[] diverseData = this.ByteWorker.Combine
            (
                this.ByteWorker.SubArray(inputData, 0, (nBlock - 1) * ConstBlockSize)
               , mLast
            );
            //
            //log.Debug("diverseData: " + HexConverter.Bytes2Hex(diverseData));
            //
            //mechanism_param_t mechanism = new mechanism_param_t();
            //data_blob_t data_in = new data_blob_t();
            //data_blob_t data_out = new data_blob_t();
            //result_t result = new result_t();
            byte[] data = diverseData;
            byte[] out_data_buf = new byte[diverseData.Length];
            //var hd0 = GCHandle.Alloc(this.iv, GCHandleType.Pinned);
            //var hd1 = GCHandle.Alloc(data, GCHandleType.Pinned);
            //var hd2 = GCHandle.Alloc(out_data_buf, GCHandleType.Pinned);
            byte[] resultBytes = null;
            try
            {
                //mechanism.mechanism = EsKmsApi.CKM_AES_CBC;
                //mechanism.parameter_len = (uint)this.iv.Length;
                //mechanism.parameter = hd0.AddrOfPinnedObject();
                ////
                //data_in.name = null;
                //data_in.type = (int)(KMS_DATA_TYPE_CODE.KMS_DATA_BLOB_TYPE_BINARY);
                //data_in.value = hd1.AddrOfPinnedObject();
                //data_in.value_len = data.Length;
                ////設定密碼運算的結果存放處
                //data_out.value = hd2.AddrOfPinnedObject();
                //data_out.value_len = out_data_buf.Length;

                //密碼運算                    
                //this.EsCryptor.Cipher(this.key, (uint)KMS_CIPHER_CODE.KMS_CIPHER_METHOD_ENCRYPT, mechanism, data_in, ref data_out, ref result);
                this.EsCryptor.CipherMode ="CBC";
                resultBytes = this.EsCryptor.Encrypt(this.keyLabel, ConstZero, data);
                //resultBytes = this.ByteWorker.SubArray(out_data_buf, 0, data_out.value_len);
                //log.Debug("resultBytes:[" + this.HexConverter.Bytes2Hex(resultBytes) + "]");
            }
            catch (Exception ex)
            {
                //log.Error(ex.StackTrace);
                throw new Exception("mac build fail:" + ex.Message);
            }
            finally
            {
                //hd0.Free();
                //hd1.Free();
                //hd2.Free();
            }
            //this.EsCryptor.release();
            return this.ByteWorker.SubArray(resultBytes, resultBytes.Length - ConstBlockSize, ConstBlockSize);
        }

        public byte[] GetMac()
        {
            byte[] fullMac = this.getFullMac(this.dataInput);
            if (this.macLength == ConstBlockSize)
            {
                return fullMac;
            }
            return this.ByteWorker.SubArray(fullMac, 0, this.macLength);
        }

        /// <summary>
        /// Every odd bytes( strat from 0 ){1,3,5,7,9,11,13,15} from 16-byte standard CMAC
        /// </summary>
        /// <returns></returns>
        public byte[] GetOdd()
        {
            byte[] fullMac = this.getFullMac(this.dataInput);
            //log.Debug("Full Mac:" + this.HexConverter.Bytes2Hex(fullMac));
            byte[] resultBytes = new byte[this.macLength / 2];
            for (int i = 0, j = 1; j < fullMac.Length; i++, j += 2)
            {
                resultBytes[i] = fullMac[j];
            }
            return resultBytes;
        }
    }
}
