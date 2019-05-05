using Microsoft.Web.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Dynamic
{
    public class CacheHubSocket : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(new CacheHubHandler());
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}