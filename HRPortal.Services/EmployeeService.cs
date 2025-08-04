using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HRPortal.Services.Models;

namespace HRPortal.Services
{
    public class EmployeeService : BaseCrudService<Employee>
    {
        public EmployeeService(HRPortal_Services services)
            : base(services)
        {
        }

        public override int Create(Employee model, bool isSave = true)
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

        public override int Update(Employee oldData, Employee newData, string[] includeProperties, bool isSave = true)
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

        public int Update(Employee oldData, Employee newData, bool isSave = true)
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
                    //string[] updatapropertiess = { "SignDepartmentID", "CellphoneNumber", "TelephoneNumber", "Email", "RoleID", "PasswordHash", "EmergencyName", "RegisterAddress", "Address", "RegisterTelephoneNumber", "EmergencyTelephoneNumber", "EmergencyAddress", "EmergencyRelation", "PasswordExpiredDate" };
                    updataproperties = new string[] { "SignDepartmentID", "CellphoneNumber", "TelephoneNumber", "Email", "RoleID", "PasswordHash", "EmergencyName", "RegisterAddress", "Address", "RegisterTelephoneNumber", "EmergencyTelephoneNumber", "EmergencyAddress", "EmergencyRelation", "EmployeesLevel", "PasswordExpiredDate", "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate", "DesignatedPerson" };
                }
                else
                {
                    //20170512 Daniel，增加更新密碼鎖定的三個欄位
                    //string[] updataproperties = { "SignDepartmentID", "CellphoneNumber", "TelephoneNumber", "Email", "RoleID", "PasswordHash", "EmergencyName", "RegisterAddress", "Address", "RegisterTelephoneNumber", "EmergencyTelephoneNumber", "EmergencyAddress", "EmergencyRelation", "EmployeesLevel" };
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

        public override int Update(Employee model, bool isSave = true)
        {
            if (model.LeaveDate != null)
            {
                //List<Department> departments = this.Services.GetService<DepartmentService>().Where().ToList();
                //departments.ForEach(x => x.contact_id = null);
                //this.Services.GetService<DepartmentService>().Update(departments, false);

                //Sys_Log sl = new Sys_Log();
                //sl.sys_id = model.sys_id;
                //sl.employee_id = GetCurrentUser().EmployeeId;
                //sl.type = (int)SysLogType.INFORMATION;
                //sl.category = "部門管理";
                //sl.title = String.Join("、", departments.Select(x => x.name).ToArray()) + " 尚未設定部門窗口";
                //this.Services.GetService<SysLogService>().Create(sl, false);
            }
            //CacheData(model.ID);
            return base.Update(model, isSave);
        }

        public override int Delete(Employee model, bool isSave = true)
        {
            //CacheData(model.ID);
            return base.Delete(model, isSave);
        }

        public override IQueryable<Employee> GetAll()
        {
            return base.GetAll().Where(x => x.Enabled == true);
        }

        public IEnumerable<Employee> GetLists(Guid companyId, Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return this.GetAll().Where(x => x.CompanyID == companyId && x.DepartmentID != null && x.DepartmentID == departmentId);
            }
            return this.GetAll().Where(x => x.CompanyID == companyId);
        }

        public IEnumerable<Employee> GetListsOfSignDepartment(Guid companyId, Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return this.GetAll().Where(x => x.CompanyID == companyId && x.SignDepartmentID != null && x.SignDepartmentID == departmentId);
            }
            return this.GetAll().Where(x => x.CompanyID == companyId);
        }

        public IEnumerable<Employee> GetListsBySignDepartment(Guid companyId, Guid? departmentId)
        {
            if (departmentId == null)
            {
                departmentId = Guid.Empty;
            }

            return this.GetAll().Where(x => x.CompanyID == companyId && x.SignDepartmentID != null && x.SignDepartmentID == departmentId);
        }


        //20210511 Daniel 增加取得傳入部門所有員工的Function，此處是傳入部門代碼清單
        public List<Employee> GetAllEmployeeByDepartment(Guid CompanyID, List<string> DepartmentCodeList, WorkingStatus WorkingStatus = WorkingStatus.InService)
        {
            List<Employee> result = new List<Employee>();

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

        public IEnumerable<Employee> GetListsOfSignDepartment(Guid companyId, IList<Department> department)
        {
            if (department != null && department.Count > 0)
            {
                List<Guid> depID = department.Select(x => x.ID).ToList();

                return this.GetAll().Where(x => x.Department.DepartmentCode != "ZZZZZZZ" && x.SignDepartmentID != null && depID.Contains(x.SignDepartmentID));
            }
            return this.GetAll().Where(x => x.CompanyID == companyId && x.Department.DepartmentCode != "ZZZZZZZ");
        }
        /// <summary>
        /// 查詢員工資料管理列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <param name="CompanyID"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<Employee> GetSearchEmployeeLists(Guid DepartmentId, Guid CompanyID, string keyword)
        {
            IQueryable<Employee> _employees;
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

        public Employee GetEmployeeByEmpNo(Guid companyId, string employeeNo)
        {
            return this.GetAll().FirstOrDefault(x => x.CompanyID == companyId && x.EmployeeNO == employeeNo);
        }

        public Employee GetEmployeeByEmpNo(string employeeNo)
        {
            return this.GetAll().FirstOrDefault(x => x.EmployeeNO == employeeNo);
        }

        //20170518 Start Daniel
        //由員編取得員工Employee物件清單，原先需要搭配公司代碼，但因目前某些補刷卡表單SignCompanyID會是Null，先不過濾公司代碼，改由過濾離職人員
        public List<Employee> GetEmployeeListByEmpNoList(List<Tuple<string, string>> EmpNoList)
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

        //20180329 Start Daniel 增加傳入工號ID，回傳Employee物件清單功能(不過濾公司ID)，含離職人員
        public List<Employee> GetSalaryEmployeeListByEmpNoList(List<string> EmpIDList)
        {
            var result = this.GetAll().Where(x => EmpIDList.Contains(x.EmployeeNO)).Include("Department").ToList();
            return result;
        }

        //20180329 End
        public IEnumerable<Employee> GetAllListsByDepartment(Guid? departmentId)
        {
            if (departmentId != null && departmentId != Guid.Empty)
            {
                return base.GetAll().Where(x => x.DepartmentID != null && x.DepartmentID == departmentId);
            }
            return base.GetAll();
        }

        //20170531 Daniel 增加由SignDepartment取得所有員工資料的方法
        public IEnumerable<Employee> GetAllListsBySignDepartment(Guid? signdepartmentId)
        {
            if (signdepartmentId != null && signdepartmentId != Guid.Empty)
            {
                return base.GetAll().Where(x => x.SignDepartmentID != null && x.SignDepartmentID == signdepartmentId);
            }
            return base.GetAll();
        }

        //public List<Guid> GetCourseTargetByEmployeeId(Guid id)
        //{
        //    string key = id.ToString() + "::CourseTarget";
        //    if (this.HasCacheData(key) == false)
        //    {
        //        return GetCourseTarget(this.Get(id));
        //    }
        //    return this.GetCacheData(key) as List<Guid>;
        //}

        //public IQueryable<Employee> GetEmployeeByCourseTarget(Course_Target target)
        //{
        //    IQueryable<Employee> query = this.Where(x => x.company.is_affiliate == false);
        //    if (target.department_condition || target.job_level_condition || target.seniority_condition || target.course_condition || target.educational_condition || target.position_condition)
        //    {
        //        //部門
        //        if (target.department_condition && target.departments.Any())
        //            query = query.Where(x => x.department_id != null && x.sys.course_targets.Any(t => t.id == target.id && t.departments.Any(d => d.department_id == x.department_id.Value)) == true);
        //        //職等
        //        if (target.job_level_condition && target.job_levels.Any())
        //            query = query.Where(x => x.job_level_id != null && x.sys.course_targets.Any(t => t.id == target.id && t.job_levels.Any(d => d.job_level_id == x.job_level_id.Value)) == true);
        //        //職位
        //        if (target.position_condition && target.positions.Any())
        //            query = query.Where(x => x.position_id != null && x.sys.course_targets.Any(t => t.id == target.id && t.positions.Any(d => d.position_id == x.position_id.Value)) == true);
        //        //教育程度
        //        if (target.educational_condition && target.educationals.Any())
        //            query = query.Where(x => x.sys.course_targets.Any(t => t.id == target.id && t.educationals.Any(d => d.educational_id == x.educational_id)) == true);
        //        if (target.seniority_condition)
        //        {
        //            DateTime max_seniority = DateTime.Today.AddYears(-target.max_seniority);
        //            DateTime min_seniority = DateTime.Today.AddYears(-target.min_seniority);
        //            if (target.min_seniority == 0)
        //                min_seniority = min_seniority.AddMonths(3); //0表示為3個月
        //            query = query.Where(x => x.arrive_date >= max_seniority && x.arrive_date <= min_seniority);
        //        }
        //        //if (target.course_condition)
        //        //{
        //        //    query = query.GroupBy(t => t.course_session.course.course_category_id).Select(x =>
        //        //        new
        //        //        {
        //        //            course_category_id = x.Key.Value,
        //        //            hours = x.Sum(t => t.course_session.times.Sum(s => s.hours))
        //        //        }).Any(x => x.course_category_id == target.course_category_id && x.hours > target.max_course_hours || x.hours < target.max_course_hours));
        //        //}
        //        if (target.course_condition)
        //        {
        //            query = query.Where(x =>
        //                x.trained_records.Where(t => t.course_session.course.course_classified_code_id == target.course_classified_code_id && t.course_session.course.course_character_id == target.course_character_id).Sum(s => s.course_session.times.Sum(h => h.hours)) <= target.max_course_hours &&
        //                x.trained_records.Where(t => t.course_session.course.course_classified_code_id == target.course_classified_code_id && t.course_session.course.course_character_id == target.course_character_id).Sum(s => s.course_session.times.Sum(h => h.hours)) >= target.min_course_hours);
        //        }
        //    }
        //    return query;
        //}

        //public List<Guid> GetCourseTarget(Employee employee)
        //{
        //    string key = employee.id.ToString() + "::CourseTarget";
        //    if (this.HasCacheData(key) == false)
        //    {
        //        List<Guid> data = new List<Guid>();
        //        foreach (Course_Target target in this.Services.GetService<CourseTargetService>().GetAll().ToList())
        //        {
        //            if (target.department_condition || target.job_level_condition || target.seniority_condition || target.course_condition || target.position_condition)
        //            {
        //                if (target.department_condition && target.departments.Any() && target.departments.Any(x => x.department_id == employee.department_id) == false)
        //                    continue;
        //                if (target.job_level_condition && target.job_levels.Any() && target.job_levels.Any(x => x.job_level_id == employee.job_level_id) == false)
        //                    continue;
        //                if (target.position_condition && target.positions.Any() && target.positions.Any(x => x.position_id == employee.position_id) == false)
        //                    continue;
        //                if (target.seniority_condition)
        //                {
        //                    DateTime zeroTime = new DateTime(1, 1, 1);
        //                    TimeSpan span = DateTime.Today - employee.arrive_date;
        //                    int years = (DateTime.Today < employee.arrive_date) ? 0 : (zeroTime + span).Year - 1;
        //                    if (years > target.max_seniority || years < target.min_seniority)
        //                        continue;
        //                }
        //                //if (item.course_condition && employee.trained_records
        //                //    .GroupBy(x => x.course_session.course.course_category_id)
        //                //    .Select(x => new { course_category_id = x.Key.Value, hours = x.Sum(t => t.course_session.times.Sum(s => s.hours)) }).Any(x => x.course_category_id == item.course_category_id && x.hours > item.max_course_hours || x.hours < item.min_course_hours) == false)
        //                //    continue;
        //                if (target.course_condition && (
        //                    employee.trained_records.Where(t => t.course_session.course.course_classified_code_id == target.course_classified_code_id && t.course_session.course.course_character_id == target.course_character_id).Sum(s => s.course_session.times.Sum(h => h.hours)) > target.max_course_hours ||
        //                    employee.trained_records.Where(t => t.course_session.course.course_classified_code_id == target.course_classified_code_id && t.course_session.course.course_character_id == target.course_character_id).Sum(s => s.course_session.times.Sum(h => h.hours)) < target.min_course_hours))
        //                    continue;
        //            }
        //            data.Add(target.id);
        //        }
        //        this.SetCacheData(key, data);
        //        return data;
        //    }
        //    return this.GetCacheData(key) as List<Guid>;
        //}

        //public virtual List<string> CheckUpdateFiled(Employee oldData, Employee newData, Dictionary<string, string> dictField)
        //{
        //    List<string> description = new List<string>();

        //    foreach (string field in dictField.Keys.ToList())
        //    {
        //        PropertyInfo oldPropertyInfo = oldData.GetType().GetProperty(field);
        //        string o = (oldPropertyInfo.GetValue(oldData) != null) ? oldPropertyInfo.GetValue(oldData).ToString() : "";

        //        PropertyInfo newPropertyInfo = newData.GetType().GetProperty(field);
        //        string n = (newPropertyInfo.GetValue(newData) != null) ? newPropertyInfo.GetValue(newData).ToString() : "";

        //        if (o != n)
        //        {
        //            if (field == "disable_date")
        //                n = n.Split(' ')[0];
        //            else if (field == "diet")
        //            {
        //                string[] new_splits = Regex.Replace(n, "[^0-9,]", "").Split(',');
        //                string[] newDiets = this.Services.GetService<OptionService>().Where(x => x.type == (int)OptionType.DIET_FOOD && new_splits.Contains(x.option_key)).Select(x => x.option_value).ToArray();
        //                n = String.Join("、", newDiets);
        //            }
        //            else if (field == "educational_id")
        //            {
        //                Option nr = this.Services.GetService<OptionService>().Get(newData.educational_id);
        //                n = (nr != null) ? nr.option_value : "";
        //            }
        //            else if (field == "role_id")
        //            {
        //                Role nr = this.Services.GetService<RoleService>().Get(newData.role_id);
        //                n = (nr != null) ? nr.name : "";
        //            }
        //            else if (field == "report_to_id" && n != "")
        //            {
        //                Employee nm = this.Get(new Guid(n));
        //                n = (nm != null) ? nm.Name : "";
        //            }
        //            else if (field == "top_executive" && n != "")
        //            {
        //                n = Boolean.Parse(n) ? "是" : "否";
        //            }

        //            description.Add(dictField[field] + ": " + n);
        //        }
        //    }

        //    return description;
        //}

        public void CacheData(Guid id)
        {
            //this.ClearRegionCacheData(id.ToString());
        }
    }
}