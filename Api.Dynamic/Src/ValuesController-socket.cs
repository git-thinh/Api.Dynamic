using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Web.Http;

namespace Api.Dynamic
{
    public class ValuesController : ApiController
    {
        static ValuesController()
        {
            wssocket_Start();
        }

        #region [ WEBSOCKET ]
        
        const string urlWS = "ws://localhost:3000/message";
        static ClientWebSocket m_socket = new ClientWebSocket();

        static void wssocket_Start()
        {
            Uri serverUri = new Uri(urlWS);
            m_socket.ConnectAsync(serverUri, CancellationToken.None).Wait();
        }

        void wssocket_Send(string text)
        {
            if (m_socket.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(text));
                m_socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            } 
        }

        #endregion

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(string _functional)
        {
            string s = "get = " + _functional;
            wssocket_Send(s);
            return s;
        }

        // POST api/values
        public string Post([FromBody]string value)
        {
            return "post = " + value;
        }

        //// PUT api/values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
    }
}