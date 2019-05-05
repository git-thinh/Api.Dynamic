using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.Web.WebSockets;
using System.Collections.Concurrent;

namespace Api.Dynamic
{
    public class CacheHubHandler : WebSocketHandler
    {
        private static Object _lock = new object();
        private static WebSocketHandler clientPush = null;
        private static ConcurrentDictionary<string, WebSocketHandler> storeFields = new ConcurrentDictionary<string, WebSocketHandler>() { };

        public static void sendMessage(string conditions) {

        }

        public override void OnMessage(string message)
        {
            switch (message)
            {
                case "CLIENT_PUSH":
                    clientPush = this;
                    break;
                default:
                    if (message.Length > 0 && message[0] == '#')
                    {
                        string field = message.Substring(1).ToUpper().Trim();
                        if (storeFields.ContainsKey(field))
                            storeFields[field] = this;
                        else
                            storeFields.TryAdd(field, this);
                    }
                    else
                    {
                        // Push query data from end-user
                        if (this.WebSocketContext.SecWebSocketKey == clientPush.WebSocketContext.SecWebSocketKey)
                        {

                        }
                        else
                        {
                            // Response return from clients cache data executed query

                        }
                    }
                    break;
            }
        }

        public override void OnOpen() { }
        public override void OnClose() { }
    }
}