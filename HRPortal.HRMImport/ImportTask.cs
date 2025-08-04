using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRPortal.HRMImport
{
    public class ImportTask : IDisposable
    {
        public Action<string> OnMessage;

        private NewHRPortalEntities _db;
        public NewHRPortalEntities DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new NewHRPortalEntities();
                }
                return _db;
            }
        }

        private Guid _userRoleId;

        public ImportTask()
        {
        }

        public ImportTask(NewHRPortalEntities db)
        {
            this._db = db;
        }

        public void WriteMessage(string message)
        {
            if(OnMessage != null)
            {
                OnMessage(message);
            }
        }

        public async Task Run()
        {
            List<Company> _companyList = DB.Companys.ToList();
            List<Department> _departmentList = DB.Departments.ToList();
            List<Employee> _employeeList = DB.Employees.ToList();

            List<CompanyData> _hrmCompany = await GetCompanyData();
            List<DepartmentData> _hrmDepartment;
            List<EmployeeData> _hrmEmployee;

            _userRoleId = DB.Roles.FirstOrDefault(x => x.Name == "一般使用者").ID;

            foreach (CompanyData _companyData in _hrmCompany)
            {
                Company _company = RenewCompanyData(_companyList, _companyData);
                _hrmDepartment = await GetDepartmentData(_companyData.Code);

                foreach (DepartmentData _departmentData in _hrmDepartment)
                {
                    Department _department = RenewDepartmentData(_departmentList, _departmentData, _company);
                    _hrmEmployee = await GetEmployeeData(_companyData.Code, _departmentData.Code);

                    foreach (EmployeeData _employeeData in _hrmEmployee)
                    {
                        RenewEmployeeData(_employeeList, _employeeData, _department);
                    }
                }

                DB.SaveChanges();
                foreach (DepartmentData _departmentData in _hrmDepartment)
                {
                    
                    Department _department = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID
                    && x.DepartmentCode == _departmentData.Code);

                    if (!string.IsNullOrEmpty(_departmentData.ParentDeptCode))
                    {
                        Department _deptList = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID
                            && x.DepartmentCode == _departmentData.ParentDeptCode);

                        if (_deptList != null)//20160518 By Bee
                        {
                            Guid _parentId = _departmentList.FirstOrDefault(x => x.CompanyID == _company.ID
                                && x.DepartmentCode == _departmentData.ParentDeptCode).ID;

                            if (!_department.ParentDepartmentID.HasValue || _department.ParentDepartmentID.Value != _parentId)
                            {
                                WriteMessage(string.Format("Set {0} parent department.", _departmentData.Code));
                                _department.ParentDepartmentID = _parentId;
                            }

                            if (!_department.SignParentID.HasValue)
                            {
                                _department.SignParentID = _parentId;
                            }
                        }
                        else if (_deptList == null) {//20160518 By Bee 
                            WriteMessage(string.Format("Set {0} parent department.", _departmentData.Code));
                            _department.ParentDepartmentID = null;
                            if (!_department.SignParentID.HasValue)
                            {
                                _department.SignParentID = null;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(_departmentData.ManagerEmpID))
                    {
                        Employee _DeptManager = _employeeList.FirstOrDefault(x => x.CompanyID == _company.ID
                            && x.EmployeeNO == _departmentData.ManagerEmpID);
                        if (_DeptManager != null)
                        {
                            Guid _managerId = _employeeList.FirstOrDefault(x => x.CompanyID == _company.ID
                                && x.EmployeeNO == _departmentData.ManagerEmpID).ID;

                            if (!_department.ManagerId.HasValue || _department.ManagerId.Value != _managerId)
                            {
                                WriteMessage(string.Format("Set {0} manager.", _departmentData.Code));
                                _department.ManagerId = _managerId;
                            }
                            if (!_department.SignManagerId.HasValue)
                            {
                                _department.SignManagerId = _managerId;
                            }
                        }

                        List<Employee> _departmentEmployeeList = _employeeList.Where(x => x.CompanyID == _department.CompanyID
                            && x.DepartmentID == _department.ID).ToList();
                        foreach (Employee _employee in _departmentEmployeeList)
                        {
                            if (_employee.ID != _department.ManagerId && _employee.ReportToID == null)
                            {
                                _employee.ReportToID = _department.ManagerId;
                            }
                        }
                    }
                }

                DB.SaveChanges();

                WriteMessage(string.Format("Company {0} setting completed.", _companyData.Code));
            }
        }

        private Company RenewCompanyData(List<Company> companyList, CompanyData companyData)
        {
            Company _company = companyList.FirstOrDefault(x => x.CompanyCode == companyData.Code);
            if (_company == null)
            {
                _company = new Company()
                {
                    ID = Guid.NewGuid(),
                    CompanyCode = companyData.Code,
                    CompanyName = companyData.Name,
                    BeginDate = DateTime.Now,
                    Enabled = true,
                    CreatedTime = DateTime.Now,
                    Company_ID = companyData.ID,
                    MainFlag=companyData.MainFlag
                };
                companyList.Add(_company);

                DB.Companys.Add(_company);
                WriteMessage(string.Format("Company {0} added.", companyData.Code));
            }
            else
            {
                var flag = 0;
                if (_company.CompanyName != companyData.Name)
                {
                    _company.CompanyName = companyData.Name;
                    flag = 1;
                   
                }
                if (_company.MainFlag != companyData.MainFlag)
                {
                    _company.MainFlag = companyData.MainFlag;
                    flag = 1;
                }
                if (flag == 1) {
                    WriteMessage(string.Format("Company {0} updated.", companyData.Code));
                }

            }
            return _company;
        }

        private Department RenewDepartmentData(List<Department> departmentList, DepartmentData departmentData, Company company)
        {
            Department _department = departmentList.FirstOrDefault(x => x.CompanyID == company.ID
                && x.DepartmentCode == departmentData.Code);
            if (_department == null)
            {
                _department = new Department()
                {
                    ID = Guid.NewGuid(),
                    DepartmentCode = departmentData.Code,
                    DepartmentName = departmentData.Name,
                    DepartmentEnglishName = departmentData.DeptNameEN,
                    CompanyID = company.ID,
                    BeginDate = DateTime.Now,
                    OnlyForSign = false,
                    Enabled = true,
                    CreatedTime = DateTime.Now,
                    Department_ID = departmentData.ID,
                    EndDate=departmentData.EndDate
                };
                departmentList.Add(_department);

                DB.Departments.Add(_department);
                WriteMessage(string.Format("Department {0} added.", departmentData.Code));
            }
            else
            {
                if (_department.DepartmentName != departmentData.Name || _department.DepartmentEnglishName != departmentData.DeptNameEN || _department.EndDate != departmentData.EndDate)
                {
                    _department.DepartmentName = departmentData.Name;
                    _department.DepartmentEnglishName = departmentData.DeptNameEN;
                    _department.EndDate = departmentData.EndDate;

                    WriteMessage(string.Format("Department {0} updated.", departmentData.Code));
                }
            }
            return _department;
        }

        private Employee RenewEmployeeData(List<Employee> employeeList, EmployeeData employeeData, Department departmnet)
        {
            Employee _employee = employeeList.FirstOrDefault(x => x.CompanyID == departmnet.CompanyID
                && x.EmployeeNO == employeeData.EmpID);

            //FEDS 更新同步程式時需要再PasswordHash欄位參數employeeData.IDNumber改成FEDS_PasswordHash(規則取身分證後五碼)by Irving 20170519
            var FEDS_PasswordHash = employeeData.IDNumber.Substring(employeeData.IDNumber.Length - 5); //取身分證最後五碼
            
            if (_employee == null)
            {
                PasswordHasher _hasher = new PasswordHasher();
                _employee = new Employee()
                {
                    ID = Guid.NewGuid(),
                    EmployeeNO = employeeData.EmpID,
                    EmployeeName = employeeData.EmpName,
                    EmployeeEnglishName=employeeData.EmpNameEN,
                    Address=employeeData.Address,
                    RegisterAddress=employeeData.RegisterAddress,//戶籍地址
                    CompanyID = departmnet.CompanyID,
                    DepartmentID = departmnet.ID,
                    SignDepartmentID = departmnet.ID,
                    Gender = (Int16)(employeeData.Sex == "M" ? 1 : 2),
                    CellphoneNumber = employeeData.Mobile,
                    TelephoneNumber = employeeData.Tel != null ? (employeeData.Tel.Length >= 17 ? null : employeeData.Tel) : null,
                    Email = employeeData.CompanyEmail,
                    ArriveDate = employeeData.AssumeDate,
                    LeaveDate = employeeData.LeaveDate,//Irving 增加離職人員到資料庫 2016/03/03
                    Enabled = true,
                    PasswordHash = _hasher.HashPassword(employeeData.IDNumber),
                    PasswordExpiredDate = DateTime.Now,
                    RoleID = _userRoleId,
                    CreatedTime = DateTime.Now,
                    EmployeeType="2",//員工類別 2:代表資料是從後台來的 1:代表資料是前台建立的
                    RegisterTelephoneNumber = employeeData.RegisterTel != null ? (employeeData.RegisterTel.Length >= 17 ? null : employeeData.RegisterTel) : null, //戶籍電話
                    EmergencyName = employeeData.EmergencyName,//緊急聯絡人姓名
                    EmergencyRelation=employeeData.EmergencyRelation,//緊急連絡人關係
                    EmergencyAddress =employeeData.EmergencyAddress,//緊急連絡人地址
                    EmergencyTelephoneNumber = (employeeData.EmergencyPhone != null ? (employeeData.EmergencyPhone.Length >= 17 ? null : employeeData.EmergencyPhone) : null),//緊急連絡人電話
                    Employee_ID=employeeData.ID//後台員工ID
                };
                employeeList.Add(_employee);

                DB.Employees.Add(_employee);
                
                WriteMessage(string.Format("Employee {0} added.", employeeData.EmpID));
                if (employeeData.EmpID == "M0060")
                {
                    string EmpID = employeeData.EmpID;
                }
            }
            else
            {
                if (_employee.EmployeeName != employeeData.EmpName || _employee.DepartmentID != departmnet.ID ||
                    _employee.TelephoneNumber != employeeData.Tel  || _employee.RegisterTelephoneNumber != employeeData.RegisterTel ||
                    _employee.CellphoneNumber != employeeData.Mobile || _employee.EmergencyTelephoneNumber != employeeData.EmergencyPhone ||
                    _employee.EmergencyAddress != employeeData.EmergencyAddress || _employee.EmergencyRelation != employeeData.EmergencyRelation ||
                    _employee.Email != employeeData.CompanyEmail || _employee.Address != employeeData.Address ||
                    _employee.RegisterAddress != employeeData.RegisterAddress || _employee.EmergencyName != employeeData.EmergencyName || _employee.LeaveDate != employeeData.LeaveDate || _employee.ArriveDate != employeeData.AssumeDate || _employee.EmployeeEnglishName != employeeData.EmpNameEN ||
                    _employee.Gender != (Int16)(employeeData.Sex == "M" ? 1 : 2))
                {   //前台名稱                //後臺名稱
                    _employee.EmployeeName = employeeData.EmpName;
                    // 20180910 小榜 判斷部門與簽核部門是否相同
                    if (_employee.DepartmentID != _employee.SignDepartmentID)
                    { //部門不等於簽核部門時，只更新所屬部門
                        _employee.DepartmentID = departmnet.ID;
                    }
                    else
                    {//部門等於簽核部門時，只更新所屬部門與簽核部門
                        _employee.DepartmentID = departmnet.ID;
                        _employee.SignDepartmentID = departmnet.ID;
                    }                   
                    _employee.TelephoneNumber = employeeData.Tel != null ? (employeeData.Tel.Length >= 17 ? null : employeeData.Tel) : null;
                    _employee.RegisterTelephoneNumber = employeeData.RegisterTel != null ? (employeeData.RegisterTel.Length >= 17 ? null : employeeData.RegisterTel) : null;
                    _employee.CellphoneNumber = employeeData.Mobile;
                    _employee.EmergencyTelephoneNumber =(employeeData.EmergencyPhone!=null?( employeeData.EmergencyPhone.Length >= 17 ? null : employeeData.EmergencyPhone):null);
                    _employee.EmergencyAddress = employeeData.EmergencyAddress;
                    _employee.EmergencyRelation = employeeData.EmergencyRelation;
                    _employee.Email = employeeData.CompanyEmail;
                    _employee.Address = employeeData.Address;
                    _employee.RegisterAddress = employeeData.RegisterAddress;
                    _employee.EmergencyName = employeeData.EmergencyName;
                    _employee.ArriveDate = employeeData.AssumeDate; //Bee 修改到職日 2017/03/15
                    _employee.LeaveDate = employeeData.LeaveDate;//Irving 增加離職人員到資料庫 2016/03/03
                    _employee.EmployeeEnglishName = employeeData.EmpNameEN;//Irving 增加英文名字到資料庫 2017/05/19
                    _employee.Gender = (Int16)(employeeData.Sex == "M" ? 1 : 2);//2018/12/26 Neo 增加性別更新到Portal資料庫
                    WriteMessage(string.Format("Employee {0} updated.", employeeData.EmpID));
                }
            }
            return _employee;
        }

        private async Task<List<CompanyData>> GetCompanyData()
        {
            return await HRMApiAdapter.GetCompany();
        }

        private async Task<List<DepartmentData>> GetDepartmentData(string companyCode)
        {
            return await HRMApiAdapter.GetDepartment(companyCode);
        }

        private async Task<List<EmployeeData>> GetEmployeeData(string companyCode, string departmentCode)
        {
            List<EmployeeData> _result = await HRMApiAdapter.GetDeptEmployee(companyCode, departmentCode);
            _result.RemoveAll(x => x.DeptCode != departmentCode);
            return _result;
        }

        public void Dispose()
        {
        }
    }
}
