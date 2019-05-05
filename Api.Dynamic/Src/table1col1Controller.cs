using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Api.Dynamic
{
    public class table1col1Controller : ApiController
    {
        const int limit = 200000;
        static CacheSynchronized<string> store = new CacheSynchronized<string>(limit);
        static table1col1Controller() {
            for (int i = 0; i < limit; i++) store.Add(i, Guid.NewGuid().ToString());
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
            int[] a = store.Search(x => x.Contains("abc"));
            return a;
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