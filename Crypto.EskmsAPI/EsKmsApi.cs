using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Crypto.EskmsAPI
{

    #region EsKmsApi的DLL所需要的結構與回傳值對照表
    public enum KMS_ERROR_CODE
    {
        KMS_ERROR_UNKNOW = 0x10000000,
        KMS_ERROR_INVALID_ARGUMENTS,
        KMS_ERROR_INCORRECT_PARAMETERS,
        KMS_ERROR_OUT_OF_MEMORY,
        KMS_ERROR_BUFFER_TOO_SMALL,
        KMS_ERROR_CONNECT,
        KMS_ERROR_NOT_AUTHENTICATION,
        KMS_ERROR_AUTHENTICATION_FAIL,
        KMS_ERROR_RESPONSE_FORMAT,
        KMS_ERROR_RESPONSE_DATA,
        KMS_ERROR_CAN_NOT_FIND_KEY,
        KMS_ERROR_CAN_NOT_FIND_DATA,
        KMS_ERROR_NO_PERMISSION,
        KMS_ERROR_DATABASE,
        KMS_ERROR_HSM,
        KMS_ERROR_KEY_STATUS_NOT_SET_OR_KEY_NOT_EXIST,
        KMS_ERROR_KEY_IS_NOT_WORKING_KEY,
        KMS_ERROR_KEY_STATUS_DISABLE,
        KMS_ERROR_KEY_STATUS_SUSPEND,
        KMS_ERROR_KEY_STATUS_REVOKE,
        KMS_ERROR_KEY_STATUS_EXPIRED,
        KMS_ERROR_PROTOCOL_INVALID,
        KMS_ERROR_LAST /* never use! */
    };

    public enum KMS_CIPHER_CODE
    {
        KMS_CIPHER_METHOD_DECRYPT = 0,
        KMS_CIPHER_METHOD_ENCRYPT,
        KMS_CIPHER_METHOD_SIGN,
        KMS_CIPHER_METHOD_VERIFY,
        KMS_CIPHER_METHOD_PKCS7_DECRYPT,
        KMS_CIPHER_METHOD_PKCS7_ENCRYPT,
        KMS_CIPHER_METHOD_PKCS7_SIGN,
        KMS_CIPHER_METHOD_PKCS7_VERIFY,
        KMS_CIPHER_METHOD_LAST /* never use! */
    };

    public enum KMS_KEY_TYPE_CODE
    {
        KMS_KEY_TYPE_KEY = 0,
        KMS_KEY_TYPE_KEY_PAIR,
        KMS_KEY_TYPE_LAST /* never use! */
    };

    public enum KMS_DATA_TYPE_CODE
    {
        KMS_DATA_BLOB_TYPE_HEX = 0,
        KMS_DATA_BLOB_TYPE_BASE64,
        KMS_DATA_BLOB_TYPE_STRING,
        KMS_DATA_BLOB_TYPE_BINARY,
        KMS_DATA_BLOB_TYPE_LAST /* never use! */
    };


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct kms_context_param_t
    {
        public string app_name;
        public string kms_server;		//KMS Interface address
        public int debug;				//0: 不輸出DEBUG訊息 1:輸出DEBUG訊息
        public int ssl;				    //0: 不使用SSL 1:使用SSL
        public int ssl_version;         //2 or 3
        public string p12file;          //pkcs12 檔案路徑
        public string p12pass;          //pkcs12 檔案密碼
        public int auth_with_cipher;	//1: 執行CIPHER時才認證
        public ulong timeout;			//傳輸TIMEOUT，單位(秒)
        public ulong connect_timeout;	//連線TIMEOUT，單位(秒)
        public string log_props_file;   //log4cxx 設定檔
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mechanism_param_t
    {
        public uint mechanism;        //PKCS#11 ck_mechanism.mechanism
        public IntPtr parameter;      //mechanism parameter, like IV
        public uint parameter_len;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct diversify_param_t
    {
        public int method;                 //diversify用方法，div 為1時才有作用，0:解密, 1:加密
        public IntPtr mechanism;           //diversify用演算法，div 為1時才有作用，參考 PKCS#11 ck_mechanism.mechanism
        public IntPtr seed;                //diversify用 seed 值，div 為1時才有作用
        public int seed_len;               //diversify用 seed 值長度，div 為1時才有作用
        public uint to_key_type;           //使用主金鑰產製出子金鑰的金鑰使用類別，div 為1時才有作用
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct result_t
    {
        public uint return_code;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string return_message;
        public uint error_code;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string error_message;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string trans_no;
    }
    /// <summary>
    /// 存放加解密時Dll所需要的資訊
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct secret_key_t
    {
        public string key_id;                   //key id
        public string key_label;                //key label, 有指定key_id時, 此參數失效
        public int key_type;                    //使用金鑰類別, reference kms_key_type_code
        public int auto_next_key;               //0:not use, 1:use (自動使用下一把有效金鑰，須搭配label使用)
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string current_key_id;           //自動使用下一把有效金鑰時，回傳使用的金鑰ID)
        public IntPtr div_param;                //diversify 子金鑰使用的演算法參數
        public uint slot;                       //HSM slot
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct cipher_param_t
    {
        public int method;					//0:decrypt, 1:encrypt, 2:sign, 3:verify
        public ulong mechanism;
        public IntPtr iv;
        public int iv_len;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct data_blob_t
    {
        public string name;         //資料名稱，可不指定
        public int type;            //資料型態
        public IntPtr value;        //資料值
        public int value_len;       //資料長度
    }
    #endregion

    /// <summary>
    /// 呼叫並執行EsKmsApi.Dll內的方法,PS:DLL要在X64下跑,所以編譯的設定都改成x64和容許UnSafe程式碼(因DLL是C++寫的)
    /// </summary>
    public class EsKmsApi : IEsKmsApi
    {
        [DllImport("eskmsapi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_version();

        [DllImport("eskmsapi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint kms_context_create(ref uint ctx, ref kms_context_param_t ctx_param);

        [DllImport("eskmsapi.dll",CallingConvention = CallingConvention.Cdecl)]
        public static extern uint kms_context_release(uint ctx);

        [DllImport("eskmsapi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint authentication(uint ctx, string appCode, string authCode);

        [DllImport("eskmsapi.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint cipher(uint ctx, ref secret_key_t key, uint cipher_method, ref mechanism_param_t mechanism, ref data_blob_t db_in, ref data_blob_t db_out, ref result_t result);

        public static readonly uint CKM_AES_ECB = 0x1081;
        public static readonly uint CKM_AES_CBC = 0x1082;
        public static readonly int KMS_NO_ERROR = 0;

        private uint ctx = 0;
        private string appCode = null;
        private string authCode = null;

        public static EsKmsApi GetInstance(kms_context_param_t ctx_param)
        {
            uint r = 0;
            uint out_ctx = 0;
            Debug.WriteLine("Begin GetInstance ...");
            //load eskmsApi.dll
            try
            {
                r = kms_context_create(ref out_ctx, ref ctx_param);
                if (r != 0)
                {
                    Debug.WriteLine(String.Format("eskmsapi error: 0x{0:X8}", r));
                    throw new Exception(String.Format("eskmsapi error: 0x{0:X8}", r));
                }
            }
            catch (Exception ex)
            {
                var qq = ex;
            }
            Debug.WriteLine(String.Format("End GetInstance:[{0:X8}]", r));
            EsKmsApi kmsapi = new EsKmsApi();
            kmsapi.ctx = out_ctx;
            return kmsapi;
        }
        #region Constructor
        public EsKmsApi() { }

        public EsKmsApi(string kmsServer, string appCode, string authCode, string appName)
        {
            //create EsKmsApi.Dll need Infomation and setting
            kms_context_param_t ctx_param = new kms_context_param_t();
            ctx_param.app_name = appName;
            ctx_param.kms_server = kmsServer;
            ctx_param.ssl = 0;
            ctx_param.debug = 0;
            ctx_param.timeout = 30;
            ctx_param.connect_timeout = 10;
            ctx_param.auth_with_cipher = 1;
            //
            uint r = 0;
            uint out_ctx = 0;//Dll return code
            r = kms_context_create(ref out_ctx, ref ctx_param);
            if (r != KMS_NO_ERROR)
            {
                string errStr = String.Format("Eskmsapi init error: 0x{0:X8}", r);
                Debug.WriteLine(errStr);
                throw new Exception(errStr);
            }
            this.ctx = out_ctx;
            Debug.WriteLine(String.Format("Eskmsapi init ok: 0x{0:X8}", this.ctx));
            this.appCode = appCode;
            this.authCode = authCode;
        }
        #endregion

        public void Authentication()
        {
            this.Authentication(this.appCode, this.authCode);
            return;
        }

        public void Authentication(string appCode, string authCode)
        {
            uint r = 0;
            this.appCode = appCode;
            this.authCode = authCode;
            //(eskmsapi.dll) run authenication method 呼叫並執行Dll內的方法並帶入參數 
            r = authentication(this.ctx, this.appCode, this.authCode);
            if (r != KMS_NO_ERROR)
            {
                string errStr = String.Format("Authentication fail:[{0:X8}]", r);
                Debug.WriteLine(errStr);
                throw new Exception(errStr);
            }
            return;
        }

        public void Cipher(secret_key_t key, uint cipher_method, mechanism_param_t mechanism, data_blob_t dbin, ref data_blob_t dbout, ref result_t result)
        {
            uint r = 0;
            r = cipher(this.ctx, ref key, cipher_method, ref mechanism, ref dbin, ref dbout, ref result);
            if (r != KMS_NO_ERROR)
            {
                string errStr = String.Format("Cipher fail:[{0:X8}]", r);
                Debug.WriteLine(errStr);
                throw new Exception(errStr);
            }
            return;
        }

        public void release()
        {
            uint r = kms_context_release(this.ctx);
            if (r != KMS_NO_ERROR)
            {
                string errStr = String.Format("Release fail:[{0:X8}]", r);
                Debug.WriteLine(errStr);
                throw new Exception(errStr);
            }
            return;
        }
    }
}
