using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace HRPortal.Core.Utilities
{
    public class RegionDataCache
    {
        public string Key { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DataCache
    {
        private static readonly IMemoryCache Cache;
        private static readonly MemoryCacheOptions Options;

        static DataCache()
        {
            Options = new MemoryCacheOptions
            {
                SizeLimit = 1024 * 1024 * 100 // 100MB
            };
            Cache = new MemoryCache(Options);
        }

        public static long CacheMemoryLimit()
        {
            return Options.SizeLimit ?? 0;
        }

        public static long PhysicalMemoryLimit()
        {
            return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        }

        private static string GetKeyName(string key, string regionName = "")
        {
            return string.IsNullOrEmpty(regionName) 
                ? key.ToUpperInvariant() 
                : $"REGION::{regionName},{key}".ToUpperInvariant();
        }

        public static List<RegionDataCache> All()
        {
            var data = new List<RegionDataCache>();
            var allKeys = GetAllKeys();
            
            var regions = allKeys
                .Where(k => k.StartsWith("REGION::", StringComparison.OrdinalIgnoreCase))
                .Select(k => k[(8)..k.IndexOf(',')])
                .Distinct();

            foreach (var regionName in regions)
            {
                data.Add(new RegionDataCache 
                { 
                    Key = regionName,
                    Count = Items(regionName).Count
                });
            }
            
            return data;
        }

        private static IEnumerable<string> GetAllKeys()
        {
            var field = typeof(MemoryCache).GetField("_entries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cache = field?.GetValue(Cache) as IDictionary<object, object>;
            return cache?.Keys.Select(k => k.ToString() ?? string.Empty) ?? Enumerable.Empty<string>();
        }

        public static List<KeyValuePair<string, object>> Items(string regionName = "")
        {
            var prefix = $"REGION::{regionName.ToUpperInvariant()},";
            return GetAllKeys()
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(k => new KeyValuePair<string, object>(k, Cache.Get(k) ?? new object()))
                .ToList();
        }

        public static dynamic? GetCacheData(string key, string regionName = "")
        {
            var cacheKey = GetKeyName(key, regionName);
            if (Cache.TryGetValue(cacheKey, out dynamic? value))
            {
                return value?.data;
            }
            return null;
        }

        public static void SetCacheData(string key, dynamic? data, string regionName = "")
        {
            var cacheKey = GetKeyName(key, regionName);
            
            if (data == null)
            {
                ClearCacheData(key, regionName);
                return;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            dynamic item = new ExpandoObject();
            item.key = key;
            item.data = data;
            item.date = DateTime.Now;

            Cache.Set(cacheKey, item, cacheEntryOptions);
        }

        public static bool HasCacheData(string key, string regionName = "")
        {
            return GetCacheData(key, regionName) != null;
        }

        public static void ClearCacheData(string key, string regionName = "")
        {
            var pattern = new Regex(
                $"REGION::{regionName.ToUpperInvariant()},{key.ToUpperInvariant()}", 
                RegexOptions.IgnoreCase);

            var keysToRemove = GetAllKeys().Where(k => pattern.IsMatch(k));
            
            foreach (var keyToRemove in keysToRemove)
            {
                Cache.Remove(keyToRemove);
            }
        }

        public static void ClearRegionCacheData(string regionName)
        {
            if (regionName.StartsWith("REGION::", StringComparison.OrdinalIgnoreCase))
                return;

            var prefix = $"REGION::{regionName.ToUpperInvariant()},";
            var keysToRemove = GetAllKeys()
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

            foreach (var keyToRemove in keysToRemove)
            {
                Cache.Remove(keyToRemove);
            }
        }
    }
}
