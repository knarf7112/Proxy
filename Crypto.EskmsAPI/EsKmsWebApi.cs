using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Net;
using Crypto.CommonUtility;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace Crypto.EskmsAPI
{
    /// <summary>
    /// 走Socket直接和KMS交訊
    /// </summary>
    public class EsKmsWebApi : IEsKmsWebApi
    {
        //
        private byte[] webHead = null;
        private byte[] webContP1 = null;
        private byte[] webContP2 = null;
        private string host = null;
        private int port = 8080;
        private IPEndPoint ipEndPoint = null;
        private string requestUri = null;
        private byte[] tailBytes = null; //new byte[] { '"', '}', '}' };
        //

        #region Property
        public IHashWorker HashWorker { private get; set; }
        public IHexConverter HexConverter { private get; set; }
        public string Url { private get; set; }
        public string HttpMethod { private get; set; }
        public string AppCode { private get; set; }
        public string AuthCode { private get; set; }
        public string AppName { private get; set; }
        public string KeyNo { private get; set; }//Key => 32A
        /// <summary>
        /// 為了選擇第一段和第二段Request的中間部分字串(暫定)
        /// "4226,\"Parameter\":\"00000000000000000000000000000000\" or "4225"
        /// </summary>
        public string CipherMode { private get; set; }//4225/ECB, 4226/CBC
        #endregion
        public EsKmsWebApi() 
        {
            this.HashWorker = new HashWorker();//用SHA1
            this.HexConverter = new HexConverter();
        }

        #region Public Method
        public byte[] Encrypt(string keyLabel, byte[] iv, byte[] decrypted)
        {
            if (null == this.webHead)
            {
                this.init();
            }
            byte[] result = null;
            //
            int contSize = this.webContP1.Length + this.webContP2.Length + decrypted.Length * 2 + this.tailBytes.Length;
            byte[] contSizeBytes = Encoding.ASCII.GetBytes("Content-Length: " + contSize + "\n\n");
            //
            byte[] request = new byte[this.webHead.Length + contSizeBytes.Length + contSize];
            int start = 0;
            Buffer.BlockCopy(this.webHead, 0, request, start, this.webHead.Length);
            start += this.webHead.Length;
            Buffer.BlockCopy(contSizeBytes, 0, request, start, contSizeBytes.Length);
            start += contSizeBytes.Length;
            Buffer.BlockCopy(this.webContP1, 0, request, start, this.webContP1.Length);
            start += this.webContP1.Length;
            Buffer.BlockCopy(this.webContP2, 0, request, start, this.webContP2.Length);
            // copy keyLabel
            byte[] keyLabelBytes = Encoding.ASCII.GetBytes(keyLabel);
            Buffer.BlockCopy(keyLabelBytes, 0, request, start + 38, keyLabelBytes.Length);
            // copy iv
            if ((null != iv) && (16 == iv.Length))
            {
                byte[] ivHexBytes = Encoding.ASCII.GetBytes(this.HexConverter.Bytes2Hex(iv));
                Buffer.BlockCopy(ivHexBytes, 0, request, start + 159, ivHexBytes.Length);//159
            }
            start += this.webContP2.Length;
            // copy decrypted
            byte[] decryptedHexBytes = Encoding.ASCII.GetBytes(this.HexConverter.Bytes2Hex(decrypted));
            Buffer.BlockCopy(decryptedHexBytes, 0, request, start, decryptedHexBytes.Length);
            start += decryptedHexBytes.Length;
            Buffer.BlockCopy(this.tailBytes, 0, request, start, this.tailBytes.Length);
            //
            Debug.WriteLine(string.Format("{0}", Encoding.ASCII.GetString(request)));
            // socket send
            using (Socket m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                m_socket.NoDelay = true;
                Debug.WriteLine("Begin connection....");
                Stopwatch timer = new Stopwatch();
                timer.Start();
                m_socket.Connect(this.ipEndPoint);
                //
                m_socket.Send(request, request.Length, SocketFlags.None);
                //
                byte[] buf = new byte[400];
                int cnt = m_socket.Receive(buf);
                //
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();
                timer.Stop();
                Debug.WriteLine("End connection... TimeSpend:" + (timer.ElapsedTicks/(decimal)Stopwatch.Frequency).ToString("f3") + "s");
                //
                string response = Encoding.UTF8.GetString(buf, 0, cnt);
                string pattern = @"""ReturnCode"":\s*(?'RC'[^,]*),.*""value"":""(?'RV'[^""]*)""";
                Regex reg = new Regex(pattern);
                Match m = reg.Match(response);

                if (m.Success && "0".Equals(m.Groups["RC"].Value))
                {
                    result = this.HexConverter.Hex2Bytes(m.Groups["RV"].Value);
                }
            }
            //
            this.webHead = null;
            if (result == null)
            {
                Debug.WriteLine("[Socket receive]Result is null");
            }
            return result;
        }

        public byte[] Decrypt(string keyLabel, byte[] iv, byte[] encrypted)
        {
            if (null == this.webHead)
            {
                this.init();
            }
            byte[] result = null;
            //
            int contSize = this.webContP1.Length + this.webContP2.Length + encrypted.Length * 2 + this.tailBytes.Length;
            byte[] contSizeBytes = Encoding.ASCII.GetBytes("Content-Length: " + contSize + "\n\n");
            //
            byte[] request = new byte[this.webHead.Length + contSizeBytes.Length + contSize];
            int start = 0;
            Buffer.BlockCopy(this.webHead, 0, request, start, this.webHead.Length);
            start += this.webHead.Length;
            Buffer.BlockCopy(contSizeBytes, 0, request, start, contSizeBytes.Length);
            start += contSizeBytes.Length;
            Buffer.BlockCopy(this.webContP1, 0, request, start, this.webContP1.Length);
            start += this.webContP1.Length;
            Buffer.BlockCopy(this.webContP2, 0, request, start, this.webContP2.Length);
            // copy keyLabel
            byte[] keyLabelBytes = Encoding.ASCII.GetBytes(keyLabel);
            Buffer.BlockCopy(keyLabelBytes, 0, request, start + 38, keyLabelBytes.Length);
            // change to decrypt
            request[start + 114] = 0x30;
            // copy iv
            if ((null != iv) && (16 == iv.Length))
            {
                byte[] ivHexBytes = Encoding.ASCII.GetBytes(this.HexConverter.Bytes2Hex(iv));
                Buffer.BlockCopy(ivHexBytes, 0, request, start + 159, ivHexBytes.Length);
            }
            start += this.webContP2.Length;
            // copy decrypted
            byte[] encryptedHexBytes = Encoding.ASCII.GetBytes(this.HexConverter.Bytes2Hex(encrypted));
            Buffer.BlockCopy(encryptedHexBytes, 0, request, start, encryptedHexBytes.Length);
            start += encryptedHexBytes.Length;
            Buffer.BlockCopy(this.tailBytes, 0, request, start, this.tailBytes.Length);

            // socket send
            using (Socket m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                m_socket.NoDelay = true;
                Debug.WriteLine("Begin connection....");
                m_socket.Connect(this.ipEndPoint);
                //
                m_socket.Send(request, request.Length, SocketFlags.None);
                //
                byte[] buf = new byte[400];
                int cnt = m_socket.Receive(buf);
                //
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();
                Debug.WriteLine("End connection...");
                //
                string response = Encoding.ASCII.GetString(buf, 0, cnt);
                string pattern = @"""ReturnCode"":\s*(?'RC'[^,]*),.*""value"":""(?'RV'[^""]*)""";
                Regex reg = new Regex(pattern);
                Match m = reg.Match(response);

                if (m.Success && "0".Equals(m.Groups["RC"].Value))
                {
                    result = this.HexConverter.Hex2Bytes(m.Groups["RV"].Value);
                }
            }
            //
            return result;
        }
        #endregion

        #region Private Method
        private void parseUrl()
        {
            string pattern = @"^http://(?'IP'[^:]*):(?'PORT'[^/]*)(?'URI'.*)$";
            Regex re = new Regex(pattern);
            Match m = re.Match(this.Url);
            if (m.Success)
            {
                this.host = m.Groups["IP"].Value;
                this.port = Convert.ToInt32(m.Groups["PORT"].Value);
                this.requestUri = m.Groups["URI"].Value;
            }
            else
            {
                string errStr = "URL match fail:[" + this.Url + "]";
                Debug.WriteLine(errStr);
                throw new Exception(errStr);
            }
            return;
        }

        private void init()
        {
            Debug.WriteLine("init...");
            string pattern = @"[0-9A-Fa-f]{40}";
            Regex checkAuthCode = new Regex(pattern);
            this.parseUrl();
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.host), this.port);
            this.tailBytes = Encoding.ASCII.GetBytes("\"}}");
            //若輸入為sha1過的密碼就直接用,若是輸入未加密的則另外做sha1和hash
            string authCodeHex = (this.AuthCode.Length == 40 && checkAuthCode.IsMatch(this.AuthCode)) ? this.AuthCode : this.HexConverter.Bytes2Hex(this.HashWorker.ComputeHash(Encoding.ASCII.GetBytes(this.AuthCode)));
            string webContStrP1 =
                "Version=1.4&AppName=" + this.AppName
              + "&AppCode=" + this.AppCode
              + "&AuthCode=" + authCodeHex
              + "&AuthWithCipher=1&Action=client&Command=key.use.do"
            ;
            this.webContP1 = Encoding.ASCII.GetBytes(webContStrP1);
            string cipherStr = (this.CipherMode == "CBC")? "4226,\"Parameter\":\"00000000000000000000000000000000\"":"4225";//CBC
            this.webContP2 = Encoding.ASCII.GetBytes
            (
                 "&JsonValue={\"WorkingKey\":{\"KeyLabel\":\"2ICH3F000010A\",\"KeyType\":0,\"KeySlot\":0,\"DiversifySeedKey\":0}"
               + ",\"CipherMethod\":1,\"Mechanism\":{\"Mechanism\":" + cipherStr + "}"
               + ",\"DataBlob\":{\"type\":0,\"value\":\""
                //0104351D22162980494341534804351D95C50B2340739FC018230F202CEDED5C\"}}"
            );
            //int contSize = this.webContP1.Length + this.webContP2.Length;
            string webHeadStr =
                this.HttpMethod + " " + this.requestUri + " HTTP/1.1\n"
              + "Host: " + this.host + ":" + this.port + "\n"
              + "Accept: */*\n"
                //+ "Content-Length: " + contSize + "\n"
              + "Content-Type: application/x-www-form-urlencoded\n"
            ;
            this.webHead = Encoding.ASCII.GetBytes(webHeadStr);
        }
        #endregion
    }
}
