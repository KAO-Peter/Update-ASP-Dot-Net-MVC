using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;

namespace HRPortal.Services.DDMC_PFA
{
    public class SystemLogService : BaseCrudService<PfaSystemLog>
    {

        public SystemLogService(HRPortal_Services services)
            : base(services)
        {
        }

        public void WriteLog(Guid _userId, string ip, string controllerName, string actionName, string remark = null)
        {
            PfaSystemLog _log = new PfaSystemLog();

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
            PfaEmployee _employee = GetCurrentEmployee();
            if(_employee != null)
            {
                WriteLog(_employee.ID, ip, controllerName, actionName, remark);
            }
        }

        public List<PfaSystemLog> GetLogRecords(DateTime _beginTime, DateTime _endTime,
            string controllerName, string actionName, Guid? _userId,Guid? _departmentId)
        {
            IQueryable<PfaSystemLog> _logRecords;

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

        public IQueryable<PfaSystemLog> GetLogList()
        {
            return this.GetAll().OrderByDescending(x=>x.LogTime);
        }
    }
}
