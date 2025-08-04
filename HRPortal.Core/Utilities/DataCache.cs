using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HRPortal.Core.Utilities
{
    public class RegionDataCache
    {
        public string key { get; set; }
        public int count { get; set; }
    }
    public class DataCache
    {
        private static MemoryCache cache = MemoryCache.Default;

        public static long CacheMemoryLimit()
        {
            return cache.CacheMemoryLimit;
        }

        public static long PhysicalMemoryLimit()
        {
            return cache.PhysicalMemoryLimit;
        }

        private static string GetKeyName(string key, string regionName = "")
        {
            if (regionName != "")
                return ("REGION::" + regionName + "," + key).ToUpper();
            else
                return key.ToUpper();
        }

        public static List<RegionDataCache> All()
        {
            List<RegionDataCache> data = new List<RegionDataCache>();
            foreach (string regionName in cache.Where(c => c.Key.IndexOf("REGION::") == 0).Select(c => c.Key.Substring(8, c.Key.IndexOf(','))).Distinct().ToList<string>())
            {
                RegionDataCache item = new RegionDataCache();
                item.key = regionName;
                item.count = Items(regionName).Count();
                data.Add(item);
            }
            return data;
        }

        public static List<KeyValuePair<string, object>> Items(string regionName = "")
        {
            return cache.Where(c => c.Key.IndexOf("REGION::" + regionName.ToUpper() + ",") == 0).ToList();
        }

        public static dynamic GetCacheData(string key, string regionName = "")
        {
            var data = (dynamic)cache[GetKeyName(key, regionName)];
            if (data == null)
                return null;
            return data.data;
        }

        public static void SetCacheData(string key, dynamic data, string regionName = "")
        {
            if (data == null)
            {
                ClearCacheData(key, regionName);
                return;
            }
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(24);
            dynamic item = new ExpandoObject();
            item.key = key;
            item.data = data;
            item.date = DateTime.Now;
            cache.Set(GetKeyName(key, regionName), item, policy);
        }

        public static Boolean HasCacheData(string key, string regionName = "")
        {
            return (GetCacheData(key, regionName) != null);
        }

        public static void ClearCacheData(string key, string regionName = "")
        {
            //if (string.IsNullOrWhiteSpace(regionName) == false)
                //regionName = "," + regionName;
            Regex rx = new Regex(string.Format(@"REGION::{0},{1}", regionName.ToUpper(), key.ToUpper()), RegexOptions.IgnoreCase);
            List<KeyValuePair<string, Object>> list = cache.Where(x => rx.IsMatch(x.Key)).ToList<KeyValuePair<string, Object>>();
            foreach (KeyValuePair<string, Object> item in list)
            {
                cache.Remove(item.Key);
            }
        }

        public static void ClearRegionCacheData(string regionName)
        {
            if (regionName.IndexOf("REGION::") == 0)
                return;
            List<KeyValuePair<string, Object>> list = cache.Where(item => item.Key.IndexOf("REGION::" + regionName.ToUpper() + ",") == 0).ToList<KeyValuePair<string, Object>>();
            foreach (KeyValuePair<string, Object> item in list)
            {
                cache.Remove(item.Key);
            }
        }
    }
}
