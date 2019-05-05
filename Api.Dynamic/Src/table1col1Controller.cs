using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
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

        void createDynamic() {
            var dataType = new Type[] { typeof(string) };
            var genericBase = typeof(List<>);
            var combinedType = genericBase.MakeGenericType(dataType);
            var listStringInstance = Activator.CreateInstance(combinedType);
            var addMethod = listStringInstance.GetType().GetMethod("Add");
            addMethod.Invoke(listStringInstance, new object[] { "Hello World" });

            var a = (new int[] { 1, 2, 3 }).AsQueryable().Where("it > 2").ToArray();  //.("@0.Contains(\"de\")");
            var a2 = (listStringInstance as List<string>).AsQueryable().Where("it.Contains(\"rl9\")").ToArray();
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

            createDynamic();

            return new int[] { };
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