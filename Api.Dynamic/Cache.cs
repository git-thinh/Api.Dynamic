using System;


namespace Api.Dynamic
{
    public interface ICache {
        string getByKey(string key);
    }

    public class StoreCache : ICache
    {
        public string getByKey(string key)
        {
            return DateTime.Now.ToString();
        }
    }
}