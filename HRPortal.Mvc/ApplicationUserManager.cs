using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.Collections.Generic;

namespace HRPortal.Mvc
{
    public class ApplicationUserManager : UserManager<ApplicationUser, Guid>
    {
        private HRPortal_Services _Services;

        public ApplicationUserManager()
            : base(new ApplicationUserSore<ApplicationUser, Guid>())
        {

        }

        public HRPortal_Services Services
        {
            get
            {
                if (_Services == null)
                    _Services = new HRPortal_Services();
                return _Services;
            }
            set
            {
                _Services = value;
            }
        }

        public static ApplicationUserManager Create(
                IdentityFactoryOptions<ApplicationUserManager> options,
                IOwinContext context)
        {
            ApplicationUserManager _userManager =new ApplicationUserManager();
            return _userManager;
        }

        public override Task<ApplicationUser> FindByIdAsync(Guid userId)
        {
            //Employee_Device device = this.Services.GetService<EmployeeDeviceService>().FirstOrDefault(x => x.id == userId);
            //if (device == null)
            //    return Task.FromResult<ApplicationUser>(null);
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == userId && x.Enabled);
            ApplicationUser user = new ApplicationUser(employee);
            user.Id = userId;
            return Task.FromResult<ApplicationUser>(user);
        }

        //public override Task<String> GetSecurityStampAsync(Guid userId)
        //{
        //    Employee_Device device = this.Services.GetService<EmployeeDeviceService>().FirstOrDefault(x => x.id == userId);
        //    if (device == null)
        //        return Task.FromResult<string>(Guid.NewGuid().ToString());
        //    return Task.FromResult<String>(device.employee.security_stamp.ToString());
        //}

        public Task<ApplicationUser> FindAsync(string employeeNo , Guid companyID, string password)
        {
            //this.Services.SetSysId(sysId);
            //MenuGroup menuGroup = (ConfigurationManager.AppSettings["AppType"].ToString() == "backend") ? MenuGroup.BACK_END : MenuGroup.FRONT_END;
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeNO == employeeNo && x.Enabled);
            return this.FindAsync(employee, password);
        }

        //public Task<ApplicationUser> FindAsync(Guid affiliate_id, string employeeNo, string password, Guid sysId)
        //{
        //    this.Services.SetSysId(sysId);
        //    MenuGroup menuGroup = (ConfigurationManager.AppSettings["AppType"].ToString() == "backend") ? MenuGroup.BACK_END : MenuGroup.FRONT_END;
        //    Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.company_id == affiliate_id && x.employee_no == employeeNo && x.role.menus.Where(m => m.menu.group == (int)menuGroup).Any());
        //    return this.FindAsync(employee, password);
        //}

        //public Task<ApplicationUser> FindAsync(LoginViewModel model, Guid sysId)
        //{
        //    if (string.IsNullOrWhiteSpace(model.login_account) == false)
        //        return FindAsync(model.login_account, model.login_password, sysId);
        //    else if (string.IsNullOrWhiteSpace(model.affiliate_account) == false)
        //        return FindAsync(model.affiliate_id, model.affiliate_account, model.affiliate_password, sysId);
        //    return Task.FromResult<ApplicationUser>(null);
        //}

        public Task<ApplicationUser> FindAsync(LoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Account) == false)
                return FindAsync(model.Account,model.Company, model.Password);
            return Task.FromResult<ApplicationUser>(null);
        }

        public async Task<ApplicationUser> FindAsync(Employee employee, string password)
        {
            if (employee == null)
                return null;

            //20170512 Start Daniel，密碼驗證改為一定回傳ApplicationUser，改由PasswordPassed屬性判斷是否通過
            bool passwordPassed = await CheckPasswordWithFailedCountAsync(employee, password);

            ApplicationUser applicationUser = new ApplicationUser(employee);
            applicationUser.PasswordPassed = passwordPassed;

            return applicationUser;
            /*
            if (await CheckPasswordAsync(employee.PasswordHash, password) == false)
                return null;
            ApplicationUser applicationUser = new ApplicationUser(employee);
            return applicationUser;
            */
            //20170512 End
        }

        //public async Task<bool> CheckPasswordAsync(Guid userId, string password)
        //{
        //    Employee_Device device = this.Services.GetService<EmployeeDeviceService>().FirstOrDefault(x => x.id == userId);
        //    return await this.CheckPasswordAsync(device.employee.password_hash, password);
        //}

        //20171221 Daniel 增加用公司ID(HRM ID)來找員工的函式
        public ApplicationUser FindByEmpIDWithHRMCompanyID(string employeeNo, int Company_ID)
        {
            int companyID;
            DateTime dateNow = DateTime.Now.Date;
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.Company.Company_ID == Company_ID && x.EmployeeNO == employeeNo && (x.LeaveDate == null || x.LeaveDate >= dateNow));

            if (employee == null)
            {
                return null;
            }
            return new ApplicationUser(employee);
        }

        public ApplicationUser FindByName(string employeeNo, Guid companyID)
        {
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeNO == employeeNo && x.Enabled);
            if (employee == null) return null;
            return new ApplicationUser(employee);
        }

        public override async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == userId && x.Enabled);
            return await this.ChangePasswordAsync(employee, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(Employee employee, string currentPassword, string newPassword)
        {
            if (await this.CheckPasswordAsync(employee.PasswordHash, currentPassword))
            {
                SetPassword(employee, newPassword);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed();
        }

        public IdentityResult SalaryChangePasswordAsync(Employee employee, string currentPassword, string newPassword)
        {
            //從 Base64 字串還原
            string _SalaryuserPW = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(currentPassword));
            if (_SalaryuserPW != newPassword)
            {
                SalarySetPassword(employee, newPassword);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed();
        }

        //public override Task<IdentityResult> UpdateSecurityStampAsync(Guid userId)
        //{
        //    Employee_Device device = this.Services.GetService<EmployeeDeviceService>().FirstOrDefault(x => x.id == userId);
        //    device.employee.security_stamp = Guid.NewGuid().ToString();
        //    this.Services.GetService<EmployeeService>().SetSysId(device.employee.sys_id);
        //    this.Services.GetService<EmployeeService>().Update(device.employee);
        //    return Task.FromResult<IdentityResult>(IdentityResult.Success);
        //}

        public string GetPasswordHash(string password)
        {
            return this.PasswordHasher.HashPassword(password);
        }

        public void SetPassword(Employee employee, string password)
        {
            //this.Services.SetSysId(employee.sys_id);
            int _expireDay = int.Parse(this.Services.GetService<SystemSettingService>().GetSettingValue("PasswordExpireDay"));
            employee.PasswordHash = GetPasswordHash(password);
            if(_expireDay > 0)
            {
                employee.PasswordExpiredDate = DateTime.Now.AddDays(_expireDay);
            }
            else
            {
                employee.PasswordExpiredDate = null;
            }

            this.Services.GetService<EmployeeService>().Update(employee);
        }

        public void SalarySetPassword(Employee employee, string password)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(password);

            //編成 Base64 字串
            string _SalaryuserPW = Convert.ToBase64String(bytes); 

            employee.SalaryPasswordHash = _SalaryuserPW; 
            employee.FirstModifiedFlag = "y";

            this.Services.GetService<EmployeeService>().Update(employee);
        }

        public void SetPassword(Guid userId, string password)
        {
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == userId && x.Enabled);
            this.SetPassword(employee, password);
        }

        public string ResetPassword(Employee employee)
        {
            //string _newPassword = GetPasswordHash(this.Services.GetService<SystemSettingService>().GetSettingValue("DefaultPassword"));
            string _newPassword = GeneratePassword();

            employee.PasswordHash = GetPasswordHash(_newPassword);
            employee.PasswordExpiredDate = DateTime.Now;
            
            //20170512 Start Daniel，忘記密碼系統自動產生新密碼時，需將密碼錯誤次數歸零，密碼鎖定狀態可以不用改，因為最前面已經判斷過了，沒鎖定才能用忘記密碼
            employee.PasswordFailedCount = 0;
            //20170512 End

            this.Services.GetService<EmployeeService>().Update(employee);

            return _newPassword;
        }

        public Employee ResetPassword(Guid userId)
        {
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == userId && x.Enabled);
            this.ResetPassword(employee);

            return employee;
        }

        private string GeneratePassword()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
        }

        public Task<bool> CheckPasswordAsync(string password_hash, string currentPassword)
        {
            if (String.IsNullOrEmpty(password_hash))
                return Task.FromResult<bool>(true);
            if (currentPassword == null)
                currentPassword = "";
            if (ConfigurationManager.AppSettings["SuperPassword"] != null && currentPassword == ConfigurationManager.AppSettings["SuperPassword"].ToString())
                return Task.FromResult<bool>(true);
            PasswordVerificationResult result = this.PasswordHasher.VerifyHashedPassword(password_hash, currentPassword);
            return Task.FromResult<bool>(result != PasswordVerificationResult.Failed);
        }

        //20170512 Start Daniel，增加檢查密碼輸入錯誤計算次數處理，錯誤次數與鎖定狀態會直接更新DB與傳入的employee物件
        public Task<bool> CheckPasswordWithFailedCountAsync(Employee employee, string currentPassword)
        {
            bool passwordCheckResult;
            bool preChecked=false;
            bool needUpdate = false;
            string password_hash = employee.PasswordHash;
            int passwordFailedCount=employee.PasswordFailedCount;
            

            //原來這段當PasswordHash是空值，直接當成驗證通過，先保留
            if (String.IsNullOrEmpty(password_hash)) 
            {
                preChecked=true;
            }

            currentPassword = currentPassword ?? "";
            
            //原來判斷有設定超級密碼，且輸入超級密碼就直接當成驗證通過，也先保留
            if (ConfigurationManager.AppSettings["SuperPassword"] != null && currentPassword == ConfigurationManager.AppSettings["SuperPassword"].ToString())
            {
                preChecked = true;
            }

            if (preChecked == true) //直接驗證通過時，設定驗證狀態為通過
            {
                passwordCheckResult=true;
            }
            else if (employee.PasswordLockStatus==true) //密碼已經被鎖定，不需再比對，直接回傳驗證不通過
            {
                passwordCheckResult=false;
            }
            else //比對密碼，並進行密碼錯誤計算次數的處理
            {
                PasswordVerificationResult result = this.PasswordHasher.VerifyHashedPassword(password_hash, currentPassword);

                List<string> includePropertiesList = new List<string>();
                Employee updatedEmployee = new Employee();

                if (result == PasswordVerificationResult.Failed) //比對密碼沒通過時
                {
                    //目前錯誤次數加一
                    passwordFailedCount++;

                    //取得密碼輸入錯誤上限次數的設定參數(PasswordFailedCountLimit)
                    int passwordFailedCountLimit;
                    int.TryParse(Services.GetService<SystemSettingService>().GetSettingValue("PasswordFailedCountLimit"),out passwordFailedCountLimit);

                    if (passwordFailedCountLimit > 0) //參數設定>0，才執行密碼鎖定計算動作
                    {
                        updatedEmployee.PasswordFailedCount = passwordFailedCount; //更新密碼錯誤次數

                        if (passwordFailedCount >= passwordFailedCountLimit) //超過上限，進行鎖定
                        {
                            updatedEmployee.PasswordLockStatus = true; //設定鎖定狀態
                            updatedEmployee.PasswordLockDate = DateTime.Now; //設定鎖定時間

                            string[] includeProperties = { "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;

                        }
                        else //還沒超過密碼錯誤上限，只更新密碼錯誤次數
                        {
                            string[] includeProperties = { "PasswordFailedCount" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;
                        }
                    }
                        
                    passwordCheckResult = false;

                }
                else //密碼比對正確時，需將原來密碼錯誤次數歸零
                {
                    if (passwordFailedCount > 0)
                    {
                        updatedEmployee.PasswordFailedCount = 0;

                        string[] includeProperties = { "PasswordFailedCount" };
                        includePropertiesList.AddRange(includeProperties);
                        needUpdate = true;
                    }

                    passwordCheckResult = true;
                }

                if (needUpdate)  //更新DB與employee物件
                {
                    EmployeeService employeeService = this.Services.GetService<EmployeeService>();
                    employeeService.Update(employee, updatedEmployee, includePropertiesList.ToArray(), true); 
                }

            }
           
            return Task.FromResult<bool>(passwordCheckResult);
        
        }
        //20170512 End

        //public Task<bool> UpdateDeviceActivityTime(Employee_Device device)
        //{
        //    device.last_activity_time = DateTime.Now;
        //    this.Services.GetService<EmployeeDeviceService>().Update(device);
        //    return Task.FromResult(true);
        //}

        //public Task<IdentityResult> SignInDeviceAsync(ApplicationUser user, string browser_type)
        //{
        //    SignOutDeviceAsync(user);
        //    string app_type = ConfigurationManager.AppSettings["AppType"].ToString();
        //    var device = this.Services.GetService<EmployeeDeviceService>().FirstOrDefault(x => x.id == user.Id && x.app_type == app_type && x.browser_type == browser_type);
        //    if (device == null)
        //    {
        //        device = new Employee_Device
        //        {
        //            employee_id = user.UserId,
        //            app_type = app_type,
        //            browser_type = browser_type,
        //            user_agent = HttpContext.Current.Request.UserAgent,
        //            ip_address = HttpContext.Current.Request.UserHostAddress,
        //            last_activity_time = DateTime.Now
        //        };
        //        this.Services.GetService<EmployeeDeviceService>().Create(device, false);
        //        this.Services.GetService<EmployeeDeviceService>().SaveChanges();
        //    }
        //    user.Id = device.id;
        //    this.Services.SetSysId(device.employee.sys_id);

        //    Employee employee = this.Services.GetService<EmployeeService>().Get(user.UserId);
        //    employee.last_login_time = DateTime.Now;
        //    this.Services.GetService<EmployeeService>().Update(employee, false);
        //    this.Services.GetService<EmployeeService>().SaveChanges();

        //    return Task.FromResult<IdentityResult>(IdentityResult.Success);
        //}

        //public Task<IdentityResult> SignOutDeviceAsync(ApplicationUser user)
        //{
        //    this.Services.GetService<EmployeeDeviceService>().Delete(x => x.id == user.Id);
        //    return Task.FromResult<IdentityResult>(IdentityResult.Success);
        //}
    }
}