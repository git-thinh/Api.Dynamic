using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Api.Dynamic
{
    public static class LinqExt
    {
        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int[] FindIndexs<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0; List<int> ls = new List<int>() { };
            foreach (var item in items)
            {
                if (predicate(item)) ls.Add(retVal);
                retVal++;
            }

            return ls.ToArray();
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }
    }

    public class CacheSynchronized<T> 
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private T[] innerCache;
        private int limit;

        public CacheSynchronized(int _limit)
        { 
            limit = _limit;
            List<T> ls = new List<T>(_limit) { };
            for (int i = 0; i < _limit; i++) ls.Add(default(T));
            innerCache = ls.ToArray();
        }

        public int Count { get { return innerCache.Length; } }

        public T Read(int index)
        {
            cacheLock.EnterReadLock();
            try
            {
                if (index < Count) return innerCache[index];
                return default(T);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public int[] Search(string keyWord)
        {
            int[] arr = new int[] { };
            cacheLock.EnterReadLock();
            try
            {
                List<int> ls = new List<int>() { };

                //for (int i = 0; i < limit; i++) if (predicate(innerCache[i])) ls.Add(i);
                var t = innerCache.Where("@0.Contains(\"cd\")").ToArray();

                arr = ls.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
            return arr;
        }

        public int[] Search(Func<T, bool> predicate)
        {
            int[] arr = new int[] { };
            cacheLock.EnterReadLock();
            try
            {
                List<int> ls = new List<int>() { };

                //for (int i = 0; i < limit; i++) if (predicate(innerCache[i])) ls.Add(i);
                Parallel.For(0, limit, i => { if (predicate(innerCache[i])) lock (ls) ls.Add(i); });

                arr = ls.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
            return arr;
        }

        public void Add(int index, T value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                if (index < Count) innerCache[index] = value;
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        //public bool AddWithTimeout(int key, string value, int timeout)
        //{
        //    if (cacheLock.TryEnterWriteLock(timeout))
        //    {
        //        try
        //        {
        //            innerCache.Add(key, value);
        //        }
        //        finally
        //        {
        //            cacheLock.ExitWriteLock();
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public AddOrUpdateStatus AddOrUpdate(int key, string value)
        //{
        //    cacheLock.EnterUpgradeableReadLock();
        //    try
        //    {
        //        string result = null;
        //        if (innerCache.TryGetValue(key, out result))
        //        {
        //            if (result == value)
        //            {
        //                return AddOrUpdateStatus.Unchanged;
        //            }
        //            else
        //            {
        //                cacheLock.EnterWriteLock();
        //                try
        //                {
        //                    innerCache[key] = value;
        //                }
        //                finally
        //                {
        //                    cacheLock.ExitWriteLock();
        //                }
        //                return AddOrUpdateStatus.Updated;
        //            }
        //        }
        //        else
        //        {
        //            cacheLock.EnterWriteLock();
        //            try
        //            {
        //                innerCache.Add(key, value);
        //            }
        //            finally
        //            {
        //                cacheLock.ExitWriteLock();
        //            }
        //            return AddOrUpdateStatus.Added;
        //        }
        //    }
        //    finally
        //    {
        //        cacheLock.ExitUpgradeableReadLock();
        //    }
        //}

        //public void Delete(int key)
        //{
        //    cacheLock.EnterWriteLock();
        //    try
        //    {
        //        innerCache.Remove(key);
        //    }
        //    finally
        //    {
        //        cacheLock.ExitWriteLock();
        //    }
        //}

        //public enum AddOrUpdateStatus
        //{
        //    Added,
        //    Updated,
        //    Unchanged
        //};

        ~CacheSynchronized()
        {
            if (cacheLock != null) cacheLock.Dispose();
        }
    }
}