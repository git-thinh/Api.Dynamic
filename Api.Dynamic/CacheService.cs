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
        private static ConcurrentDictionary<string, WebSocketService> m_storeFields = new ConcurrentDictionary<string, WebSocketService>() { };
        private static string m_storeIDs = string.Empty;
        private static string[] m_fields = new string[] { };

        private static ConcurrentDictionary<string, WebSocketService> m_searchSender = new ConcurrentDictionary<string, WebSocketService>() { };
        private static ConcurrentDictionary<string, oCacheSearchFieldRseult> m_searchItemResults = new ConcurrentDictionary<string, oCacheSearchFieldRseult>() { };
        private static ConcurrentDictionary<string, int> m_searchItemTotal = new ConcurrentDictionary<string, int>() { };
        private static ConcurrentDictionary<string, int> m_searchItemCounter = new ConcurrentDictionary<string, int>() { };

        void fetchCacheItems(string conditions)
        {
            try
            {
                oCacheSearchField[] searchs = JsonConvert.DeserializeObject<oCacheSearchField[]>(conditions);
                if (searchs.Length > 0)
                {
                    string searchId = Guid.NewGuid().ToString();

                    m_searchSender.TryAdd(searchId, this);
                    m_searchItemTotal.TryAdd(searchId, searchs.Length);

                    string find; WebSocketService cache;
                    for (int i = 0; i < searchs.Length; i++)
                    {
                        searchs[i].SearchId = searchId;
                        find = JsonConvert.SerializeObject(searchs[i]);
                        if (m_storeFields.TryGetValue(searchs[i].Field, out cache)) {
                            try
                            {
                                cache.Send(find);
                            }
                            catch(Exception exx) {
                                m_searchItemResults.TryAdd(searchId + searchs[i].Index, new oCacheSearchFieldRseult() {
                                    Ok = false,
                                    Message = exx.Message,
                                    Index = searchs[i].Index,
                                    SearchId = searchId
                                });

                                if (m_searchItemCounter.ContainsKey(searchId)) {
                                    int counter = 0;
                                    if (m_searchItemCounter.TryGetValue(searchId, out counter))
                                        m_searchItemCounter.TryUpdate(searchId, counter + 1, counter);
                                } else {
                                    m_searchItemCounter.TryAdd(searchId, 1);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex) {
                this.Send(new oCacheSearchResult(false, "EXCEPTION", this.WebSocketContext.SecWebSocketKey) { Output = ex.Message, Conditions = conditions }.toJson());
            }
        }

        void processMessage(string m)
        {
            if (string.IsNullOrWhiteSpace(m)) return;
            m = m.Trim();
            if (m.Length < 2) return;

            //Send BroadCast query data from end-user
            if (m[0] == '[' && m[m.Length - 1] == ']')
            {
                fetchCacheItems(m);
                return;
            }

            string code = m.ToUpper();
            switch (code)
            {
                case "TABLE":
                    this.Send(new oCacheSearchResult(true, code, this.WebSocketContext.SecWebSocketKey) { Output = JsonConvert.SerializeObject(m_fields) }.toJson());
                    break;
                default:
                    switch (m[0])
                    {
                        case '#':
                            // Register TABLE.FIELD_NAME by text is #TABLE.FIELD_NAME
                            string field = m.Substring(1).ToUpper().Trim();
                            if (field.Length > 0)
                            {
                                if (m_storeFields.ContainsKey(field))
                                {
                                    WebSocketService val;
                                    m_storeFields.TryRemove(field, out val);
                                }
                                m_storeFields.TryAdd(field, this);
                                lock (m_storeIDs) m_storeIDs += ";" + this.WebSocketContext.SecWebSocketKey;
                            }
                            break; 
                    }
                    break;
            }
        }

        public override void OnOpen() { }
        public override void OnMessage(string message) => processMessage(message);
        public override void OnMessage(Byte[] buffer) { }
        protected override void OnClose()
        {
            lock (m_storeIDs) m_storeIDs.Replace(";" + this.WebSocketContext.SecWebSocketKey, string.Empty);
            base.OnClose();
        }
        protected override void OnError()
        {
            base.OnError();
        }
    }
}