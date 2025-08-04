using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Helpers;
using HRPortal.DBEntities.DDMC_PFA;

namespace HRPortal.Services.DDMC_PFA
{
    public class UserClaimsPrincipal : ClaimsPrincipal
    {
        protected HRPortal_Services Services;

        public UserClaimsPrincipal(IPrincipal principal, HRPortal_Services services)
            : base(principal)
        {
            Services = services;
            if(!string.IsNullOrEmpty(Employee.Role.RoleParams))
            {
                _roleParams = Json.Decode(Employee.Role.RoleParams);
            }
        }
        private List<PfaDepartment> GetDepartment()
        {
            List<PfaDepartment> _result = new List<PfaDepartment>();

            List<PfaDepartment> _allDepartment = Services.GetService<DepartmentService>().Where(x => x.CompanyID == Employee.CompanyID && !x.OnlyForSign && x.Enabled).ToList();
            List<PfaDepartment> _foundDepartmnet = _allDepartment.Where(x => x.ManagerId == Employee.ID).ToList();

            while (_foundDepartmnet != null && _foundDepartmnet.Count > 0)
            {
                _result.AddRange(_foundDepartmnet);
                //2019/03/05 Neo ParentDepartmentID改為SignParentID直接抓簽核的單位
                _foundDepartmnet = _allDepartment.Where(x => x.SignParentID.HasValue
                    && _foundDepartmnet.Select(y => y.ID).Contains(x.SignParentID.Value)).ToList();
            }

            return _result.Distinct().OrderBy(x => x.DepartmentCode).ToList();
        }

        private List<PfaDepartment> GetSignDepartment()
        {
            List<PfaDepartment> _result = new List<PfaDepartment>();

            List<PfaDepartment> _allDepartment = Services.GetService<DepartmentService>().Where(x => x.CompanyID == Employee.CompanyID && x.Enabled).ToList();
            List<PfaDepartment> _foundDepartmnet = _allDepartment.Where(x => x.SignManagerId == Employee.ID).ToList();

            while (_foundDepartmnet != null && _foundDepartmnet.Count > 0)
            {
                _result.AddRange(_foundDepartmnet);
                _foundDepartmnet = _allDepartment.Where(x => x.SignParentID.HasValue
                    && _foundDepartmnet.Select(y => y.ID).Contains(x.SignParentID.Value)).ToList();
            }

            return _result.Distinct().OrderBy(x => x.DepartmentCode).ToList();
        }

        public List<PfaDepartment> _departments;
        public List<PfaDepartment> Departments 
        {
            get
            {
                if (_departments == null)
                    _departments = GetDepartment();
                return _departments;
            }
         }

        public List<PfaDepartment> _signDepartments;
        public List<PfaDepartment> SignDepartments
        {
            get
            {
                if (_signDepartments == null)
                    _signDepartments = GetSignDepartment();
                return _signDepartments;
            }
        }

        private dynamic _roleParams;

        public bool IsAdmin
        {
            get
            {
                return (_roleParams != null && _roleParams.is_admin != null && _roleParams.is_admin);
            }
        }

        public bool IsHR
        {
            get
            {
                return (_roleParams != null && _roleParams.is_hr != null && _roleParams.is_hr);
            }
        }

        public Guid EmployeeID
        {
            get
            {
                if (this.FindFirst("EmployeeID") == null)
                    return Guid.Empty;
                return new Guid(this.FindFirst("EmployeeID").Value);
            }
        }

        public string CompanyName
        {
            get
            {
                return this.FindFirst("CompanyName").Value;
            }
        }

        //20190528 Daniel 增加公司英文名稱欄位
        public string CompanyEnglishName
        {
            get
            {
                return this.FindFirst("CompanyEnglishName").Value;
            }
        }

        public string Name
        {
            get
            {
                return this.FindFirst(ClaimTypes.Name).Value;
            }
        }

        public virtual string SystemName
        {
            get
            {
                return this.CompanyName + " HR Portal";//this.FindFirst("SystemName").Value;
            }
        }

        //20190528 Daniel 增加系統英文名稱
        public  string SystemEnglishName
        {
            get
            {
                return this.CompanyEnglishName + " HR Portal";//this.FindFirst("SystemName").Value;
            }
        }
                
        private PfaEmployee _employee;

        public virtual PfaEmployee Employee
        {
            get
            {
                if (_employee == null)
                {
                    _employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == this.EmployeeID);
                }
                return _employee;
            }
        }

        public string EmployeeNO
        {
            get
            {
                return Employee.EmployeeNO;
            }
        }

        public Guid CompanyID
        {
            get
            {
                return Employee.CompanyID;
            }
        }

        public string CompanyCode
        {
            get
            {
                return Employee.Company.CompanyCode;
            }
        }

        public Guid DepartmentID
        {
            get
            {
                return Employee.DepartmentID;
            }
        }

        public Guid SignDepartmentID
        {
            get
            {
                return Employee.SignDepartmentID;
            }
        }

        public string DepartmentCode
        {
            get
            {
                return Employee.Department.DepartmentCode;
            }
        }

        public string SignDepartmentCode
        {
            get
            {
                return Employee.SignDepartment.DepartmentCode;
            }
        }

        private List<PfaMenu> _menus;

        public virtual List<PfaMenu> Menus
        {
            get
            {
                if (_menus == null)
                {
                    using (MenuService service = Services.GetService<MenuService>())
                    {
                        _menus = service.GetFullMenu(service.Db.Roles.FirstOrDefault(x => x.ID == this.Employee.RoleID)).ToList<PfaMenu>();
                    }
                }
                return _menus;
            }
        }

        public void ClearMenus()
        {

        }

        public bool HasMenus(string contentPath)
        {
            return Menus.Any(x => x.Link == contentPath);
        }
    }
}
