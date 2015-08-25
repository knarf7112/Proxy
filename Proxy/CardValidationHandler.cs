using System.Text;
//
using System.Web;
using ALCommon;
using Newtonsoft.Json;

namespace Proxy
{
    /// <summary>
    /// 檢核卡號有效性,連到AP的檢查服務6103
    /// </summary>
    public class CardValidationHandler : IHttpHandler
    {
        /// <summary>
        /// 傳入卡號,去後台AP檢查有效性(port:6103)
        /// 000000:Pass/990001:後台錯誤/990003:黑名單/990012:非有效卡或非聯名卡或非正常卡
        /// </summary>
        /// <param name="icc_No"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns>Return Code(若傳輸異常回傳null)</returns>
        public string CheckCard(string icc_No,string ip,int port = 6103)
        {
            AL2POS_Domain queryObj = new AL2POS_Domain()
            {
                ICC_NO = icc_No,
                READER_ID = string.Empty,
                REG_ID = string.Empty,
                STORE_NO = string.Empty,
                AL_AMT = 0,
            };
            string queryStr = JsonConvert.SerializeObject(queryObj);
            byte[] queryRequest = Encoding.UTF8.GetBytes(queryStr);
            byte[] queryResponse;
            using (SocketClient.Domain.SocketClient client = new SocketClient.Domain.SocketClient(ip,port))
            {
                queryResponse = client.SendAndReceive(queryRequest);
            }
            if (queryResponse != null)
            {
                string responseStr = Encoding.UTF8.GetString(queryResponse);
                AL2POS_Domain response = JsonConvert.DeserializeObject<AL2POS_Domain>(responseStr);
                return response.AL2POS_RC;
            }
            else
            {
                return null;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            string icc_No = context.Request.QueryString["icc_no"].ToString();

            string returnCode = this.CheckCard(icc_No, "10.27.68.155");//ip=測試機IP


        }
    }
}
