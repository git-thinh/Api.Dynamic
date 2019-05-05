using Microsoft.ServiceModel.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Api.Dynamic
{
    public class oCacheSearchResult
    {
        public bool Ok { set; get; }
        public string SendId { set; get; }
        public string Code { set; get; }

        [JsonIgnore]
        public string Output { set; get; }

        public oCacheSearchResult(bool ok, string code, string sendId)
        {
            this.Ok = ok;
            this.SendId = sendId;
            this.Code = code;
            this.Output = string.Empty;
        }

        public string toJson()
        {
            return @"{""Output"":" + Output + "," + JsonConvert.SerializeObject(this).Substring(1);
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
        private static ConcurrentDictionary<string, WebSocketService> storeFields = new ConcurrentDictionary<string, WebSocketService>() { };
        private static string storeIDs = string.Empty;

        void fetchCacheItems(string conditions)
        {

        }

        void processMessage(string message)
        {
            // Register TABLE.FIELD_NAME by text is #TABLE.FIELD_NAME
            if (message.Length > 0 && message[0] == '#')
            {
                string field = message.Substring(1).ToUpper().Trim();
                if (storeFields.ContainsKey(field))
                    storeFields[field] = this;
                else
                    storeFields.TryAdd(field, this);
                lock (storeIDs) storeIDs += ";" + this.WebSocketContext.SecWebSocketKey;
            }
            else
            {
                bool isReturnFetchCacheItems = false;
                lock (storeIDs) isReturnFetchCacheItems = storeIDs.Contains(this.WebSocketContext.SecWebSocketKey);
                // Response return from clients cache data executed query
                if (isReturnFetchCacheItems)
                {

                }
                else
                {
                    if (message.ToLower() == "table")
                        this.Send(new oCacheSearchResult(true, "TABLE", this.WebSocketContext.SecWebSocketKey) { Output = string.Join(";", storeFields.Keys) }.toJson());
                    else
                        fetchCacheItems(message);// send BroadCast query data from end-user
                }
            }
        }

        public override void OnOpen() { }
        public override void OnMessage(string message) => processMessage(message);
        public override void OnMessage(Byte[] buffer) { }
        protected override void OnClose()
        {
            lock (storeIDs) storeIDs.Replace(";" + this.WebSocketContext.SecWebSocketKey, string.Empty);
            base.OnClose();
        }
        protected override void OnError()
        {
            base.OnError();
        }
    }
}