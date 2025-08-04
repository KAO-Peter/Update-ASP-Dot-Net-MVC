using HRPortal.Core;
using HRPortal.Core.Utilities;
//using HRPortal.Models;
using HRPortal.DBEntities;
using NLog;
using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Transactions;

namespace HRPortal.Services
{
    public abstract class BaseService : IDisposable
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        protected HRPortal_Services Services;
        private NewHRPortalEntities _db;
        //private BaseService _baseService;
        private Guid? _sysId;
        private UserClaimsPrincipal _currentUser;
        private Employee _currentEmployee;

        public BaseService(HRPortal_Services services)
        {
            //_baseService = baseService;
            Services = services;
        }

        public NewHRPortalEntities Db
        {
            get
            {
                return Services.Db;
            }
        }

        public Employee GetCurrentEmployee()
        {
            //if (_baseService != null)
            //    return _baseService.GetCurrentEmployee();

            if (_currentEmployee == null)
            {
                Guid EmployeeId = GetCurrentUser().EmployeeID;
                _currentEmployee = Db.Employees.FirstOrDefault(x => x.ID == EmployeeId);
            }
            return _currentEmployee;
        }

        //public string GetAppFileDirectory(params string[] paths)
        //{
        //    string AppFileDirectory = ConfigurationManager.AppSettings["AppFileDirectory"].ToString();
        //    if (AppFileDirectory.IndexOf('.') == 0)
        //    {
        //        AppFileDirectory = Path.Combine(HttpContext.Current.Server.MapPath("~"), AppFileDirectory);
        //    }
        //    return Path.Combine(AppFileDirectory, GetSysId().ToString(), Path.Combine(paths));
        //}

        public void SaveFile(string path, string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return;
            SaveFile(path, Convert.FromBase64String(base64));
        }

        public void SaveFile(string path, Stream stream)
        {
            if (stream == null)
                return;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            SaveFile(path, bytes);
        }

        public void SaveFile(string path, byte[] bytes)
        {
            if (bytes == null)
                return;
            if (bytes.Length == 0)
                return;
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Directory.Exists)
                fileInfo.Delete();
            else
                fileInfo.Directory.Create();
            System.IO.File.WriteAllBytes(fileInfo.ToString(), bytes);
        }

        public void DeleteFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Directory.Exists)
                fileInfo.Delete();
        }

        public virtual int SaveChanges(bool isSave = true)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //if (_baseService != null)
                    //    return _baseService.SaveChanges(isSave);

                    if (isSave == false)
                        return 0;

                    this.Services.SyncClearCacheData();
                    int _result = Db.SaveChanges();
                    scope.Complete();
                    return _result;
                }
                
                catch
                {
                    throw;
                }
                
                /*
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            // raise a new exception nesting
                            // the current instance as InnerException
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
                */
 
            }
        }

        public UserClaimsPrincipal GetCurrentUser()
        {
            //if (_baseService != null)
            //    return _baseService.GetCurrentUser();

            if (_currentUser == null && HttpContext.Current != null)
            {
                if (HttpContext.Current.User != null)
                    _currentUser = new UserClaimsPrincipal(HttpContext.Current.User, Services);
            }
            return _currentUser;
        }

        public void SetSysId(Guid? id)
        {
            _sysId = id;
        }

        //public Guid? GetSysId()
        //{
        //    if (_baseService != null)
        //        return _baseService.GetSysId();

        //    if (_sysId == null)
        //    {
        //        if (GetCurrentUser() != null && GetCurrentUser().Identity.IsAuthenticated)
        //            SetSysId(this.GetCurrentUser().SysId);
        //    }
        //    return _sysId;
        //}

        string _regionName = string.Empty;

        public string GetRegionName()
        {
            if (string.IsNullOrWhiteSpace(_regionName))
                _regionName = this.GetType().Name;
            return _regionName;
        }

        public void ClearRegionCacheData(string key = "")
        {
            if (key == "")
                key = GetRegionName();
            else
                key = GetRegionName() + "," + key;

            this.Services.ClearCacheData(key, "");

            DataCache.ClearRegionCacheData(key);
        }

        public void ClearCacheData(string key)
        {
            this.Services.ClearCacheData(GetRegionName(), key);

            DataCache.ClearCacheData(key, GetRegionName());
        }

        public dynamic GetCacheData(string key)
        {
            return DataCache.GetCacheData(key, GetRegionName());
        }

        public void SetCacheData(string key, dynamic data)
        {
            DataCache.SetCacheData(key, data, GetRegionName());
        }

        public Boolean HasCacheData(string key)
        {
            //if (this.GetSysId() == null)
                //return false;
            return DataCache.HasCacheData(key, GetRegionName());
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

            //if (disposing)
            //{
            //    if (_baseService != null)
            //    {
            //        _baseService.Dispose();
            //        _baseService = null;
            //    }
            //    if (Services != null)
            //    {
            //        Services.Dispose();
            //        Services = null;
            //    }
            //}
        }

        ~BaseService()
        {
            Dispose(false);
        }
    }
}