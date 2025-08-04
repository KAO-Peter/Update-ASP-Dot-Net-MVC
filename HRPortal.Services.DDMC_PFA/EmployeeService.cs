using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;

namespace HRPortal.Services.DDMC_PFA
{
    public class EmployeeService : BaseCrudService<PfaEmployee>
    {
        public EmployeeService(HRPortal_Services services)
            : base(services)
        {
        }

        public override int Create(PfaEmployee model, bool isSave = true)
        {
            try
            {
                model.ID = Guid.NewGuid();
                model.CreatedTime = DateTime.Now;
                model.DepartmentID = model.SignDepartmentID;
                model.Enabled = true;
                model.TopExecutive = false;
                return base.Create(model, isSave);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override int Update(PfaEmployee oldData, PfaEmployee newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            result = base.Update(oldData, newData, includeProperties, isSave);

            return result;
        }

        public int Update(PfaEmployee oldData, PfaEmployee newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties;
                //增加判斷只有更改密碼才會彈出視窗 Irving 2016/03/29
                if (newData.PasswordHash != oldData.PasswordHash)
                {
                    newData.PasswordExpiredDate = DateTime.Now;
                    //20170512 Daniel，增加更新密碼鎖定的三個欄位，並補上EmployeesLevel欄位
                    updataproperties = new string[] { "SignDepartmentID", "CellphoneNumber", "TelephoneNumber", "Email", "RoleID", "PasswordHash", "EmergencyName", "RegisterAddress", "Address", "RegisterTelephoneNumber", "EmergencyTelephoneNumber", "EmergencyAddress", "EmergencyRelation", "EmployeesLevel", "PasswordExpiredDate", "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate", "DesignatedPerson" };
                }
                else
                {
                    //20170512 Daniel，增加更新密碼鎖定的三個欄位
                    updataproperties = new string[] { "SignDepartmentID", "CellphoneNumber", "TelephoneNumber", "Email", "RoleID", "PasswordHash", "EmergencyName", "RegisterAddress", "Address", "RegisterTelephoneNumber", "EmergencyTelephoneNumber", "EmergencyAddress", "EmergencyRelation", "EmployeesLevel", "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate", "DesignatedPerson" };
                }
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception)
            {
                result = 0;
            }

            return result;
        }

        public override int Update(PfaEmployee model, bool isSave = true)
        {
            if (model.LeaveDate != null)
            {
            }
            return base.Update(model, isSave);
        }

        public override int Delete(PfaEmployee model, bool isSave = true)
        {
            return base.Delete(model, isSave);
        }

        public override IQueryable<PfaEmployee> GetAll()
        {
            return base.GetAll().Where(x => x.Enabled == true);
        }

        public IEnumerable<PfaEmployee> GetLists(Guid companyId, Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return this.GetAll().Where(x => x.CompanyID == companyId && x.DepartmentID != null && x.DepartmentID == departmentId);
            }
            return this.GetAll().Where(x => x.CompanyID == companyId);
        }

        public IEnumerable<PfaEmployee> GetListsOfSignDepartment(Guid companyId, Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return this.GetAll().Where(x => x.CompanyID == companyId && x.SignDepartmentID != null && x.SignDepartmentID == departmentId);
            }
            return this.GetAll().Where(x => x.CompanyID == companyId);
        }

        public IEnumerable<PfaEmployee> GetListsBySignDepartment(Guid companyId, Guid? departmentId)
        {
            if (departmentId == null)
            {
                departmentId = Guid.Empty;
            }

            return this.GetAll().Where(x => x.CompanyID == companyId && x.SignDepartmentID != null && x.SignDepartmentID == departmentId);
        }

        public IEnumerable<PfaEmployee> GetListsOfSignDepartment(Guid companyId, IList<PfaDepartment> department)
        {
            if (department != null && department.Count > 0)
            {
                List<Guid> depID = department.Select(x => x.ID).ToList();

                return this.GetAll().Where(x => x.Department.DepartmentCode != "ZZZZZZZ" && x.SignDepartmentID != null && depID.Contains(x.SignDepartmentID));
            }
            return this.GetAll().Where(x => x.CompanyID == companyId && x.Department.DepartmentCode != "ZZZZZZZ");
        }

        //20210511 Daniel 增加取得傳入部門所有員工的Function，此處是傳入部門代碼清單
        public List<PfaEmployee> GetAllEmployeeByDepartment(Guid CompanyID, List<string> DepartmentCodeList, WorkingStatus WorkingStatus = WorkingStatus.InService)
        {
            List<PfaEmployee> result = new List<PfaEmployee>();

            if (DepartmentCodeList.Count > 0)
            {
                var query = this.GetAll().Where(x => DepartmentCodeList.Contains(x.Department.DepartmentCode));

                DateTime now = DateTime.Now;

                switch (WorkingStatus)
                {
                    case WorkingStatus.InService:
                        query = query.Where(x => x.LeaveDate == null || x.LeaveDate >= now);
                        break;
                    case WorkingStatus.NotWorking:
                        query = query.Where(x => x.LeaveDate.HasValue && x.LeaveDate < now);
                        break;
                    default:
                        break;
                }

                query = query.OrderBy(x => x.Department.DepartmentCode).ThenBy(y => y.EmployeeNO);
                result = query.ToList();
            }

            return result;
        }

        /// <summary>
        /// 查詢員工資料管理列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <param name="CompanyID"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<PfaEmployee> GetSearchEmployeeLists(Guid DepartmentId, Guid CompanyID, string keyword)
        {
            IQueryable<PfaEmployee> _employees;
            _employees = GetAll();
            if (CompanyID != Guid.Empty)
            {
                _employees = _employees.Where(x => x.CompanyID == CompanyID);
            }

            if (DepartmentId != Guid.Empty)
            {
                _employees = _employees.Where(x => x.SignDepartmentID == DepartmentId);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                _employees = _employees.Where(x => x.EmployeeNO.Contains(keyword) || x.EmployeeName.Contains(keyword));
            }
            return _employees.OrderBy(x => x.CompanyID).ThenBy(x => x.SignDepartmentID).ThenBy(x => x.EmployeeNO).ToList();
        }

        public PfaEmployee GetEmployeeByEmpNo(Guid companyId, string employeeNo)
        {
            return this.GetAll().FirstOrDefault(x => x.CompanyID == companyId && x.EmployeeNO == employeeNo);
        }

        public PfaEmployee GetEmployeeByEmpNo(string employeeNo)
        {
            return this.GetAll().FirstOrDefault(x => x.EmployeeNO == employeeNo);
        }

        //20170518 Start Daniel
        //由員編取得員工PfaEmployee物件清單，原先需要搭配公司代碼，但因目前某些補刷卡表單SignCompanyID會是Null，先不過濾公司代碼，改由過濾離職人員
        public List<PfaEmployee> GetEmployeeListByEmpNoList(List<Tuple<string, string>> EmpNoList)
        {
            List<string> tempEmpNoList = EmpNoList.Select(x => x.Item1).ToList();
            //以下是加上過濾公司代碼的跟不抓離職的語法，先註解掉
            /*
            var result = from emp in this.GetAll().Where(x => tempEmpNoList.Contains(x.EmployeeNO) && x.LeaveDate != null).ToList()
                         from eno in EmpNoList
                         where emp.EmployeeNO == eno.Item1
                         && (emp.Company.CompanyCode == eno.Item2 || eno.Item2 == null) //先不過濾公司代碼
                         select emp;
            */

            //目前只抓還沒離職的員工需要寄信通知
            var result = this.GetAll().Where(x => tempEmpNoList.Contains(x.EmployeeNO) && x.LeaveDate == null).ToList();

            return result;
        }

        //20170518 End

        //20180329 Start Daniel 增加傳入工號ID，回傳PfaEmployee物件清單功能(不過濾公司ID)，含離職人員
        public List<PfaEmployee> GetSalaryEmployeeListByEmpNoList(List<string> EmpIDList)
        {
            var result = this.GetAll().Where(x => EmpIDList.Contains(x.EmployeeNO)).Include("Department").ToList();
            return result;
        }

        //20180329 End
        public IEnumerable<PfaEmployee> GetAllListsByDepartment(Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return base.GetAll().Where(x => x.DepartmentID != null && x.DepartmentID == departmentId);
            }
            return base.GetAll();
        }

        //20170531 Daniel 增加由SignDepartment取得所有員工資料的方法
        public IEnumerable<PfaEmployee> GetAllListsBySignDepartment(Guid? signdepartmentId)
        {
            if (signdepartmentId != null && signdepartmentId != Guid.Empty)
            {
                return base.GetAll().Where(x => x.SignDepartmentID != null && x.SignDepartmentID == signdepartmentId);
            }
            return base.GetAll();
        }

        public void CacheData(Guid id)
        {
        }
    }
}