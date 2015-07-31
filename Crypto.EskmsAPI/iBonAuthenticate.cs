using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using Crypto.CommonUtility;
using System.Threading.Tasks;

namespace Crypto.EskmsAPI
{
    public class iBonAuthenticate
    {
        #region Field
        private IHexConverter hexConverter;

        private IByteWorker byteWorker;

        private IEsKmsWebApi esKmsWebApi;
        #endregion

        #region Constructor
        public iBonAuthenticate()
        {
            this.hexConverter = new HexConverter();
            this.byteWorker = new ByteWorker();
            this.esKmsWebApi = new EsKmsWebApi()
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
        #endregion


        public void StartAuthenticate()
        {
            
        }
    }
}
