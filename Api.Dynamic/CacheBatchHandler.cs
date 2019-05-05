using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Batch;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Api.Dynamic
{
    public class CacheBatchHandler : BatchHandler
    {
        private const string TextJson = "text/json";
        private const string ApplicationJsonContentType = "application/json";

        /// <inheritdoc />
        public CacheBatchHandler(HttpServer httpServer) : base(httpServer)
        {
            SupportedContentTypes.Add(TextJson);
            SupportedContentTypes.Add(ApplicationJsonContentType);
        }

        /// <inheritdoc />
        public override async Task<IList<HttpRequestMessage>> ParseBatchRequestsAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            //string data = await request.Content.ReadAsStringAsync(); 
            var cacheSubRequests = await request.Content.ReadAsAsync<CacheRequestMessage[]>(cancellationToken);
            string requestId = Guid.NewGuid().ToString();

            // Creating simple requests, and check for the body
            var subRequests = cacheSubRequests.Select((r, index) =>
            {
                r.Index = index;
                r.RequestId = requestId;

                string path = "/api/" + r.Field.Replace(".", string.Empty).ToLower() + "?RequestId=" + r.RequestId + "&RequestIndex=" + r.Index;
                var subRequestUri = new Uri(request.RequestUri, path);
                var req = new HttpRequestMessage(new HttpMethod("GET"), subRequestUri);

                // Add the body
                var content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, ApplicationJsonContentType);
                req.Content = content;

                // Add the headers
                foreach (var item in request.Headers)
                    req.Headers.Add(item.Key, item.Value);

                req.CopyBatchRequestProperties(request);

                return req;
            });

            return subRequests.ToList();
        }

        /// <inheritdoc />
        public override async Task<HttpResponseMessage> CreateResponseMessageAsync(IList<HttpResponseMessage> responses,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responses == null)
            {
                throw new ArgumentNullException(nameof(responses));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            List<CacheResponseMessage> listResponses = new List<CacheResponseMessage>();
            foreach (var subResponse in responses)
            {
                var dic = subResponse.RequestMessage.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
                string requestId = string.Empty, requestIndex = "0";
                if (dic.ContainsKey("RequestId")) requestId = dic["RequestId"];
                if (dic.ContainsKey("RequestIndex")) requestIndex = dic["RequestIndex"];

                var res = new CacheResponseMessage()
                {
                    Code = (int)subResponse.StatusCode,
                    Index = int.Parse(requestIndex),
                    RequestId = requestId
                };

                if (subResponse.StatusCode == HttpStatusCode.OK)
                {
                    res.Ok = true;
                    if (subResponse.Content != null) res.Output = await subResponse.Content.ReadAsAsync<int[]>();
                }
                else
                {
                    if (subResponse.Content != null) res.Message = await subResponse.Content.ReadAsStringAsync();
                }

                listResponses.Add(res);
            }
            return request.CreateResponse(HttpStatusCode.OK, listResponses);
        }
    }

    public class CacheRequestMessage
    {
        public string RequestId { set; get; }
        public int Index { set; get; }
        public string Field { set; get; }
        public string Condition { set; get; }
        public string OrderBy { set; get; }
    }

    public class CacheResponseMessage
    {
        public CacheResponseMessage()
        {
            Output = new int[] { };
            Ok = false;
        }

        public string RequestId { set; get; }
        public int Index { set; get; }

        public bool Ok { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public int[] Output { get; set; }
    }

}