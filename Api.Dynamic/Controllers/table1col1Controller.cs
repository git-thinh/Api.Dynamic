using Google.ProtocolBuffers.Rpc;
using helloworld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Api.Dynamic
{
    public class table1col1Controller : ApiController
    {
        const int limit = 200000;
        static CacheSynchronized<string> store = new CacheSynchronized<string>(limit);

        static readonly Guid iid = Marshal.GenerateGuidForType(typeof(IGreeter));
        static readonly Greeter _client;

        //////static readonly ClientWebSocket _client = new ClientWebSocket();
        static table1col1Controller()
        {
            for (int i = 0; i < limit; i++) store.Add(i, Guid.NewGuid().ToString());
            //////try
            //////{
            //////    _client.ConnectAsync(new Uri("ws://localhost:56049/message"), CancellationToken.None).Wait();
            //////    if (_client.State == WebSocketState.Open)
            //////    {
            //////        string msg = Guid.NewGuid().ToString();
            //////        ArraySegment<byte> bytesToSend = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(msg));
            //////        _client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            //////    }
            //////}
            //////catch { }
            ///

            try {
                //_client = new Greeter(RpcClient.ConnectRpc(iid, "ncacn_ip_tcp", @"localhost", "50051").Authenticate(RpcAuthenticationType.None));
                _client = new Greeter(RpcClient.ConnectRpc(iid, "ncacn_ip_tcp", @"localhost", "50051").Authenticate(RpcAuthenticationType.Self));
                HelloReply response = _client.SayHello(HelloRequest.CreateBuilder().SetName(Guid.NewGuid().ToString()).Build());
                Debug.WriteLine("OK: " + response.Message);
            }
            catch { }

            //using (Greeter client = new Greeter(RpcClient
            //                    .ConnectRpc(iid, "ncacn_ip_tcp", @"localhost", "50051")
            //                    .Authenticate(RpcAuthenticationType.Self)
            //                    //.Authenticate(RpcAuthenticationType.None)
            //                    ))
            //{
            //    HelloReply response = client.SayHello(HelloRequest.CreateBuilder().SetName(Guid.NewGuid().ToString()).Build());
            //    Console.WriteLine("OK: " + response.Message);
            //}
        }

        void createDynamic()
        {
            //var dataType = new Type[] { typeof(string) };
            //var genericBase = typeof(List<>);
            //var combinedType = genericBase.MakeGenericType(dataType);
            //var listStringInstance = Activator.CreateInstance(combinedType,);
            //var addMethod = listStringInstance.GetType().GetMethod("Add");
            //addMethod.Invoke(listStringInstance, new object[] { "Hello World" });

            //var a = (new int[] { 1, 2, 3 }).AsQueryable().Where("it > 2").ToArray();  //.("@0.Contains(\"de\")");
            //var a2 = (listStringInstance as List<string>).AsQueryable().Where("it.Contains(\"rl9\")").ToArray();

            object x = new Int32[7];
            Type t = x.GetType();
            object y = Array.CreateInstance(t.GetElementType(), 7);

            Type t2 = typeof(int);
            object y2 = Array.CreateInstance(t2, 7);

            Type t3 = typeof(string);
            object y3 = Array.CreateInstance(t3, 7);
            for (int i = 0; i < 7; i++) (y3 as string[])[i] = Guid.NewGuid().ToString();
        }

        // GET api/values
        public IEnumerable<int> Get()
        {            
            return new int[] { 0, 1 };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "id = " + id;
        }

        // POST api/values
        public IEnumerable<int> Post([FromBody]CacheRequestMessage value)
        {
            //int[] a = store.Search(x => x.Contains("abc"));

            //////createDynamic();

            //////if (_client.State == WebSocketState.Open)
            //////{
            //////    string msg = Guid.NewGuid().ToString();
            //////    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(msg));
            //////    _client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            //////}

            HelloReply response = _client.SayHello(HelloRequest.CreateBuilder().SetName(Guid.NewGuid().ToString()).Build());
            Debug.WriteLine("OK: " + response.Message);

            return new int[] { 3, 4, 5, 6 };
        }

        // PUT api/values/5
        public string Put(int id, [FromBody]string value)
        {
            return "id = " + id + "; value = " + value;
        }

        // DELETE api/values/5
        public string Delete(int id)
        {
            return "id = " + id;
        }
    }
}