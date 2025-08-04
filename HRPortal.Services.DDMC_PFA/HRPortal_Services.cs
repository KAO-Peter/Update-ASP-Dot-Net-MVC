using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities.DDMC_PFA;

namespace HRPortal.Services.DDMC_PFA
{
    public class HRPortal_Services : IDisposable
    {
        private List<Tuple<string, string>> _cachNameDate;
        private Dictionary<Type, BaseService> _serviceDictionary;
        private string _backendBaseUrl;
        private string _frontendBaseUrl;

        NewHRPortalEntitiesDDMC_PFA _db;
        public NewHRPortalEntitiesDDMC_PFA Db
        {
            get
            {
                return _db;
            }
        }

        public HRPortal_Services()
        {
            _cachNameDate = new List<Tuple<string, string>>();
            _serviceDictionary = new Dictionary<Type, BaseService>();
            _db = new NewHRPortalEntitiesDDMC_PFA();
        }

        public void ClearCacheData(string regionName, string key)
        {
            if (_cachNameDate.Any(x => x.Item1 == regionName && x.Item2 == key) == false)
                _cachNameDate.Add(Tuple.Create<string, string>(regionName, key));
        }

        public void SyncClearCacheData()
        {
            if (_cachNameDate.Count == 0)
                return;
            SyncClearCacheDataPost(GetFrontendBaseUrl(), _cachNameDate.ToArray());
            _cachNameDate.Clear();
        }

        private static void SyncClearCacheDataPost(string baseAddress, Tuple<string, string>[] CachNameDate)
        {
            foreach (Tuple<string, string> CachNameItem in CachNameDate)
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("regionName", CachNameItem.Item1));
                postData.Add(new KeyValuePair<string, string>("key ", CachNameItem.Item1));

                HttpContent content = new FormUrlEncodedContent(postData);
                Task.Run(() =>
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(baseAddress + "/");
                        try
                        {
                            HttpResponseMessage resp = client.PostAsync("api/Cache/ClearCacheData", content).Result;
                        }
                        catch{}
                    }
                });
            }
        }

        private string GetFrontendBaseUrl()
        {
            if (string.IsNullOrWhiteSpace(_frontendBaseUrl))
                _frontendBaseUrl = ConfigurationManager.AppSettings["BaseUrl"].ToString();//GetService<ConfigService>().GetConfig(Config.FRONTEND_BASE_URL);
            return _frontendBaseUrl;
        }

        public TBaseService GetService<TBaseService>() where TBaseService : BaseService
        {
            Type type = typeof(TBaseService);
            object[] argValues = new object[] { this };
            if (_serviceDictionary.ContainsKey(type) == false)
                _serviceDictionary.Add(type, (TBaseService)Activator.CreateInstance(type, argValues));
            return (TBaseService)_serviceDictionary[type];
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            disposed = true;

            if (disposing)
            {
                if (_serviceDictionary != null)
                {
                    foreach (var item in _serviceDictionary)
                    {
                        item.Value.Dispose();
                    }
                }
                SyncClearCacheData();
            }
        }

        ~HRPortal_Services()
        {
            Dispose(false);
        }
    }
}
