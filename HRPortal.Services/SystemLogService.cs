//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class SystemLogService : BaseCrudService<SystemLog>
    {

        public SystemLogService(HRPortal_Services services)
            : base(services)
        {
        }

        public void WriteLog(Guid _userId, string ip, string controllerName, string actionName, string remark = null)
        {
            SystemLog _log = new SystemLog();

            _log.ID = Guid.NewGuid();
            _log.LogTime = DateTime.Now;

            _log.UserID = _userId;
            _log.UserIP = ip;
            _log.Controller = controllerName;
            _log.Action = actionName;
            _log.Remark = remark;

            Create(_log);
        }

        public void WriteLog(string ip, string controllerName, string actionName, string remark = null)
        {
            Employee _employee = GetCurrentEmployee();
            if(_employee != null)
            {
                WriteLog(_employee.ID, ip, controllerName, actionName, remark);
            }
        }

     
        public List<SystemLog> GetLogRecords(DateTime _beginTime, DateTime _endTime,
            string controllerName, string actionName, Guid? _userId,Guid? _departmentId)
        {
            IQueryable<SystemLog> _logRecords;

            _logRecords = GetAll().Where(x => x.LogTime >= _beginTime && x.LogTime <= _endTime);
            if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
            {
                _logRecords = _logRecords.Where(x => x.Controller == controllerName
                    && x.Action == actionName);
            }
            if (_userId != null && _userId !=Guid.Empty)
            {
                _logRecords = _logRecords.Where(x => x.UserID == _userId);
            }
            if (_departmentId != null && _departmentId != Guid.Empty)
            {
                _logRecords = _logRecords.Where(x => x.Employee.DepartmentID == _departmentId);
            }

            return _logRecords.OrderByDescending(x=>x.LogTime).ToList();
        }

        public IQueryable<SystemLog> GetLogList()
        {
            return this.GetAll().OrderByDescending(x=>x.LogTime);
        }

    
    }
}
