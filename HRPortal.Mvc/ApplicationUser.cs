using System;
using Microsoft.AspNet.Identity;
using HRPortal.DBEntities;
using HRPortal.Services;
using System.Linq;
using System.Collections.Generic;
//using HRPortal.Models.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;


namespace HRPortal.Mvc
{
    // You can add profile model for the user by adding more properties to your ApplicationUser class, 
    //please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationUser : IUser<Guid>
    {
        public ApplicationUser(Employee employee)
        {
            _employee = employee;
            this.Id = employee.ID;

            if (!string.IsNullOrEmpty(Employee.Role.RoleParams))
            {
                _roleParams = Json.Decode(Employee.Role.RoleParams);
            }
        }

        private Employee _employee;

        public Employee Employee
        {
            get { return _employee; }
        }

        public Guid Id { get; set; }

        public Guid UserId
        {
            get
            {
                return Employee.ID;
            }
        }

        public string UserName
        {
            get
            {
                return Employee.EmployeeName;
            }
            set
            {
                ;
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

        //20170512 Daniel，增加登入密碼驗證是否通過的註記
        public bool PasswordPassed { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            userIdentity.AddClaim(new Claim("ID", this.Id.ToString()));
            userIdentity.AddClaim(new Claim("EmployeeID", this.UserId.ToString()));
            userIdentity.AddClaim(new Claim("EmployeeNO", this.Employee.EmployeeNO));
            userIdentity.AddClaim(new Claim("CompanyCode", Employee.Company.CompanyCode));
            userIdentity.AddClaim(new Claim("CompanyName", Employee.Company.CompanyName));
            userIdentity.AddClaim(new Claim("DepartmentCode", Employee.Department.DepartmentCode));
            userIdentity.AddClaim(new Claim("DepartmentName", Employee.Department.DepartmentName));
            userIdentity.AddClaim(new Claim("SignDepartmentCode", Employee.SignDepartment.DepartmentCode));
            userIdentity.AddClaim(new Claim("SignDepartmentName", Employee.SignDepartment.DepartmentName));

            //20190528 Daniel 取得公司英文名稱
            CompanyData companyData = await HRMApiAdapter.GetCompanyByCode(Employee.Company.CompanyCode);
            string companyEnglishName = companyData == null ? Employee.Company.CompanyName : (string.IsNullOrWhiteSpace(companyData.CompanyEnglishName) ? Employee.Company.CompanyName : companyData.CompanyEnglishName);
            userIdentity.AddClaim(new Claim("CompanyEnglishName", companyEnglishName));
          
            return userIdentity;
        }
    }

}