using Microsoft.ServiceModel.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Api.Dynamic
{
    public class oCacheSearchResult
    {
        public bool Ok { set; get; }
        public string SendId { set; get; }
        public string SearchId { set; get; }
        public string Code { set; get; }
        public string Conditions { set; get; }

        [JsonIgnore]
        public string Output { set; get; }

        public oCacheSearchResult(bool ok, string code, string sendId)
        {
            this.Ok = ok;
            this.SendId = sendId;
            this.Code = code;
            this.Output = string.Empty;
            this.SearchId = string.Empty;
        }

        public oCacheSearchResult(string sendId, string cacheId)
        {
            this.Ok = false;
            this.SendId = sendId;
            this.Code = "SEARCH";
            this.SearchId = cacheId;
            this.Output = string.Empty;
        }

        public string toJson()
        {
            return @"{""Output"":" + Output + "," + JsonConvert.SerializeObject(this).Substring(1);
        }
    }

    public class oCacheSearchField
    {
        public int Index { set; get; }
        public string Field { set; get; }
        public string Condition { set; get; }
        public string OrderBy { set; get; }
        public string SearchId { set; get; }
    }

    public class oCacheSearchFieldRseult
    {
        public bool Ok { set; get; }
        public int Index { set; get; }
        public string SearchId { set; get; }
        public int[] Output { set; get; }
        public string Message { set; get; }

        public oCacheSearchFieldRseult()
        {
            this.Ok = false;
            this.Index = 0;
            this.SearchId = string.Empty;
            this.Output = new int[] { };
            this.Message = string.Empty;
        }
    }

    public class CacheServiceFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            WebSocketHost host = new WebSocketHost(serviceType, baseAddresses);
            host.AddWebSocketEndpoint();
            return host;
        }
    }

    public class CacheService : WebSocketService
    {
        public override void OnOpen() {
            Debug.WriteLine("CONNECTED ...");
        }
        public override void OnMessage(string message) {
            Debug.WriteLine("->: " + message);
        }

        public override void OnMessage(Byte[] buffer) { }
        protected override void OnClose()
        {
            base.OnClose();
        }
        protected override void OnError()
        {
            base.OnError();
        }
    }
}