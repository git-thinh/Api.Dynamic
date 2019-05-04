using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Api.Dynamic.Controllers.ExportExcel
{
    #region [ UDP ]

    public struct Received
    {
        public IPEndPoint Sender;
        public string Message;
    }

    abstract class UdpBase
    {
        protected UdpClient Client;

        protected UdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }
    }

    //Server
    class UdpListener : UdpBase
    {
        private IPEndPoint _listenOn;

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, 32123))
        {
        }

        public UdpListener(IPEndPoint endpoint)
        {
            _listenOn = endpoint;
            Client = new UdpClient(_listenOn);
        }

        public void Reply(string message, IPEndPoint endpoint)
        {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length, endpoint);
        }

    }

    //Client
    class UdpUser : UdpBase
    {
        private UdpUser() { }

        public static UdpUser ConnectTo(string hostname, int port)
        {
            var connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }

        public void Send(string message)
        {
            var datagram = Encoding.ASCII.GetBytes(message);
            Client.Send(datagram, datagram.Length);
        }

    }
    
    #endregion

    public class ExportExcelController : ApiController
    {
        static ExportExcelController()
        {
            try
            {
                cache_Init();
                //namedPipe_Start();
                udp_Start();
                wssocket_Start();
            }
            catch { }
        }

        #region [ CACHE ]

        static readonly ConcurrentQueue<string> m_cacheData = new ConcurrentQueue<string>();
        const int m_cacheTimerInterval = 100;
        const int m_cacheTimerIntervalSleep = 1000;
        static readonly Lazy<Timer> m_cacheTimer = new Lazy<Timer>(() => new Timer((state) =>
        {
            if (m_cacheData.Count > 0)
            {
                string data;
                if (m_cacheData.TryDequeue(out data))
                {
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        cache_processDataOnQueue(data);
                    }
                }
                m_cacheTimer.Value.Change(TimeSpan.FromMilliseconds(m_cacheTimerInterval), TimeSpan.FromMilliseconds(-1));
            }
            else
            {
                m_cacheTimer.Value.Change(TimeSpan.FromMilliseconds(m_cacheTimerIntervalSleep), TimeSpan.FromMilliseconds(-1));
            }
            //To set timer with random interval
            //m_cacheTimer.Value.Change(TimeSpan.FromMilliseconds(randNum.Next(1, 3) * 500), TimeSpan.FromMilliseconds(-1));
        }, null, 0, m_cacheTimerInterval));

        static void cache_Init()
        {
            Timer t = m_cacheTimer.Value;
        }

        static void cache_processDataOnQueue(string data)
        {
            Debug.WriteLine("QUEUE: " + data);
            ssePushData(data);
        }

        #endregion

        #region [ NAMED PIPE ]

        static void namedPipe_Start()
        {
            //var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            //var rule = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow);
            //var sec = new PipeSecurity();
            //sec.AddAccessRule(rule);
            //pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 100, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, sec);

            //Task.Run(() =>
            //{
            //    pipeServer.WaitForConnection();

            //    var read = 0;
            //    var bytes = new byte[4096];
            //    while ((read = pipeServer.Read(bytes, 0, bytes.Length)) > 0)
            //    {
            //        Debug.WriteLine(string.Join(" ", bytes));
            //    }
            //});

            Task.Factory.StartNew(() =>
            {
                var server = new NamedPipeServerStream("PipesOfPiece");
                server.WaitForConnection();
                StreamReader reader = new StreamReader(server);
                //StreamWriter writer = new StreamWriter(server);
                while (true)
                {
                    var data = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        m_cacheData.Enqueue(data);
                        Debug.WriteLine("PIPE: " + data);
                        //writer.WriteLine("SERVER: " + String.Join("", data.Reverse()));
                        //writer.Flush();
                        Thread.Sleep(100);
                    }
                }
            });
        }



        #endregion

        #region [ UDP ]

        static void udp_Start() {
            //create a new server
            var server = new UdpListener();

            //start listening for messages and copy the messages back to the client
            Task.Factory.StartNew(async () => {
                while (true)
                {
                    var received = await server.Receive();
                    //server.Reply("copy " + received.Message, received.Sender);
                    //if (received.Message == "quit") break;

                    m_cacheData.Enqueue(received.Message);
                    Debug.WriteLine("UDP: " + received.Message);
                    //writer.WriteLine("SERVER: " + String.Join("", data.Reverse()));
                    //writer.Flush();
                    Thread.Sleep(100);
                }
            });
        }

        #endregion

        #region [ WEBSOCKET ]

        static ClientWebSocket m_socket = new ClientWebSocket();
        static void wssocket_Start()
        {
            Uri serverUri = new Uri("ws://localhost:56049/message");
            m_socket.ConnectAsync(serverUri, CancellationToken.None).Wait();
            //if (socket.State == WebSocketState.Open)
            //{
            //    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data));
            //    socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
            //}
        }

        #endregion

        #region [ SSE ]

        static readonly ConcurrentQueue<StreamWriter> _streammessage = new ConcurrentQueue<StreamWriter>();

        static void ssePushData(string text)
        {
            foreach (var data in _streammessage)
            {
                data.WriteLine("data:" + text + "\n");
                data.Flush();
            }
        }

        HttpResponseMessage getSSE()
        {
            HttpRequestMessage request = this.ActionContext.Request;

            //Timer t = _timer.Value;
            HttpResponseMessage response = request.CreateResponse();
            response.Content = new PushStreamContent((stream, headers, context) =>
            {
                StreamWriter streamwriter = new StreamWriter(stream);
                _streammessage.Enqueue(streamwriter);
            }, "text/event-stream");
            return response;
        }

        #endregion

        #region [ BASE ]

        [HttpGet] // api/exportexcel/{_functional}?id=...
        public HttpResponseMessage Get(string _functional)
        {
            //var dicPara = this.ActionContext.Request.GetQueryNameValuePairs().ToLookup(x => x.Key, x => x.Value);
            //var dicPara = this.ActionContext.Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
            return processRequest(_functional);
        }

        [HttpPost] // api/exportexcel
        public HttpResponseMessage Post()
        {
            //string _functional = "get_all";
            string _functional = "";
            var queryString = this.ActionContext.Request.RequestUri.Query;
            if (!string.IsNullOrWhiteSpace(queryString))
                _functional = HttpUtility.ParseQueryString(queryString.Substring(1))["_functional"];
            return processRequest(_functional);
        }

        protected override void Dispose(bool disposing)
        {
            if (m_socket.State == WebSocketState.Open) m_socket.Abort();
            freeResource();
            base.Dispose(disposing);
        }

        #endregion

        #region [ MAIN - ROUNTER ]

        void freeResource()
        {
            //db.Dispose();
        }

        HttpResponseMessage processRequest(string functional)
        {
            switch (functional)
            {
                case "sse": return getSSE();
                case "get_all": return getAll();
            }
            var response = Request.CreateResponse(HttpStatusCode.Created);
            return response;
        }

        #endregion

        #region [ LOGIC - BIZ ]

        HttpResponseMessage getAll()
        {
            var response = Request.CreateResponse(HttpStatusCode.Created);
            response.Content = new StringContent(JsonConvert.SerializeObject(new List<string> { "ASP.NET", "Docker", "Windows Containers" }));
            return response;
        }

        #endregion
    }
}
