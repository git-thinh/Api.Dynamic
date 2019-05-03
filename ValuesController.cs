using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Api.Dynamic
{
    public class ValuesController : ApiController
    {
        static ClientWebSocket socket = new ClientWebSocket();
        static ValuesController()
        {
            Uri serverUri = new Uri("ws://localhost:56049/message");
            socket.ConnectAsync(serverUri, CancellationToken.None).Wait(); 
        }

        public IEnumerable<string> Get()
        {
            string data = HttpContext.Current.Items["data_session"] as string;

            if (socket.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data));
                socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            //string data = "";// HttpContent.Current.Session["data_session"] as string;
            return new List<string> { "ASP.NET", "Docker", "Windows Containers", data };
        }
    }
}
