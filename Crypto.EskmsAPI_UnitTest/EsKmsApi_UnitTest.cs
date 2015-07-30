using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//
using Crypto.CommonUtility;
using Crypto.EskmsAPI;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Crypto.EskmsAPI_UnitTest
{
    [TestClass]
    public class EsKmsApi_UnitTest
    {
        private IHexConverter hexConverter;
        private IByteWorker byteWorker;
        private EsKmsApi esKmsApi;

        [TestInitialize]
        public void InitContext()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();

            //KMS參數設定
            kms_context_param_t ctx_param = new kms_context_param_t();
            ctx_param.app_name = "kms2Test";
            ctx_param.kms_server = //@"http://10.27.68.163:8080/eGATEsKMS/interface";
                @"http://127.0.0.1:8081/eGATEsKMS/interface";
            ctx_param.ssl = 0;
            ctx_param.debug = 1;
            ctx_param.timeout = 30;
            ctx_param.connect_timeout = 10;
            ctx_param.auth_with_cipher = 1;
            //DLL要在X64下跑,所以編譯的設定都改成x64和容許UnSafe程式碼(因DLL是C++寫的),測試設定的預設處理器架構也要改x64
            this.esKmsApi = EsKmsApi.GetInstance(ctx_param);
        }

        [TestMethod]
        public void Test_01Authenticate()
        {
            //測試傳輸
            this.esKmsApi.Authentication("APP_001", "12345678");
        }

        [TestMethod]
        public void Test_02DiverseK0()
        {
            this.esKmsApi.Authentication("APP_001", "12345678");

            //
            secret_key_t key = new secret_key_t();
            key.key_label = "2ICH3F000010A";
            key.key_type = (int)KMS_KEY_TYPE_CODE.KMS_KEY_TYPE_KEY;
            key.slot = 0;
            //
            mechanism_param_t mechanism = new mechanism_param_t();
            data_blob_t data_in = new data_blob_t();
            data_blob_t data_out = new data_blob_t();
            result_t result = new result_t();
            byte[] out_data_buf = new byte[1024];
            //byte[] iv = this.byteWorker.Fill(16, 0x00);
            //this.hexConverter.Hex2Bytes("4B6E6F4BF5539AB5BFE5D679EA1ED16E");
            byte[] data = this.byteWorker.Fill(16, 0x00);
            //Encoding.ASCII.GetBytes( this.hexConverter.Bytes2Hex( this.byteWorker.Fill(16, 0x00) ) );
            //var hd1 = GCHandle.Alloc( iv, GCHandleType.Pinned);
            //規劃一塊Unmanaged的記憶體空間提供給Dll寫入並固定住記憶體位置,避免.NET整理記憶體空間時,把目前Address轉移到其他Address,造成Dll寫入或讀取時弄錯記憶體Address
            var hd1 = GCHandle.Alloc(data, GCHandleType.Pinned);//放置要給Dll取得的參數資料
            var hd2 = GCHandle.Alloc(out_data_buf, GCHandleType.Pinned);//放置Dll計算完後回傳的資料
            try
            {
                //取得記憶體位置
                key.div_param = hd1.AddrOfPinnedObject();//
                //設定加密的區塊模式
                mechanism.mechanism = EsKmsApi.CKM_AES_ECB;
                //mechanism.parameter = hd1.AddrOfPinnedObject();
                //mechanism.parameter_len = (uint)iv.Length;
                //
                data_in.name = null;
                data_in.type = (int)(KMS_DATA_TYPE_CODE.KMS_DATA_BLOB_TYPE_BINARY);//BLOB傳輸方式使用Binary
                data_in.value = hd1.AddrOfPinnedObject();//取得放input資料的記憶體Address
                data_in.value_len = data.Length;
                //設定密碼運算的結果存放處
                data_out.value = hd2.AddrOfPinnedObject();//取得放output資料的記憶體Address
                data_out.value_len = out_data_buf.Length;

                //密碼運算
                this.esKmsApi.Cipher(key, (uint)KMS_CIPHER_CODE.KMS_CIPHER_METHOD_ENCRYPT, mechanism, data_in, ref data_out, ref result);

                byte[] k0 = this.byteWorker.SubArray(out_data_buf, 0, data_out.value_len);

                Debug.WriteLine("K0:[" + this.hexConverter.Bytes2Hex(k0) + "]");
                Debug.WriteLine(String.Format("{0}:{1}:{2}:{3}:{4}", result.error_code, result.error_message, result.return_code, result.return_message, result.trans_no));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            finally
            {
                hd1.Free();//釋放Unmanaged的記憶體空間
                hd2.Free();//釋放Unmanaged的記憶體空間
            }
        }

        [TestCleanup]
        public void Clear()
        {
            Debug.WriteLine("Dispose kmsapi...");
            try
            {
                this.esKmsApi.release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
