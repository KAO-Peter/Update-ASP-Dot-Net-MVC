using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Areas.M01.Controllers
{
    public class PersonalSettingsController : BaseController
    {
        //
        // GET: /M01/PersonalSetting/
        public ActionResult Index()
        {
            List<PersonalSettings> model = Services.GetService<PersonalSettingsService>().GetParameter("HomePage", CurrentUser.CompanyCode, CurrentUser.EmployeeNO).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string ToDoList, string SalarySearch, string PersonalLeavesSummary, string LeaveToday)
        {
            IEnumerable<PersonalSettings> _personalSettings = Services.GetService<PersonalSettingsService>().GetParameter("HomePage", CurrentUser.CompanyCode, CurrentUser.EmployeeNO);

            UpdatePersonalSettings(_personalSettings, "ToDoList", ToDoList);
            UpdatePersonalSettings(_personalSettings, "SalarySearch", SalarySearch);
            UpdatePersonalSettings(_personalSettings, "PersonalLeavesSummary", PersonalLeavesSummary);
            UpdatePersonalSettings(_personalSettings, "LeaveToday", LeaveToday);

            TempData["message"] = "儲存成功";
            _personalSettings = Services.GetService<PersonalSettingsService>().GetParameter("HomePage", CurrentUser.CompanyCode, CurrentUser.EmployeeNO);
            return View(_personalSettings.ToList());

        }

        private void UpdatePersonalSettings(IEnumerable<PersonalSettings> _personalSettings, string key, string value)
        {
            PersonalSettings _ps = _personalSettings.FirstOrDefault(x => x.SettingKey == key);
            if (_ps == null)
            {
                PersonalSettings ps = new PersonalSettings()
                {
                    ID = Guid.NewGuid(),
                    Type = "HomePage",
                    CompanyCode = CurrentUser.CompanyCode,
                    EmployeeNO = CurrentUser.EmployeeNO,
                    SettingKey = key,
                    SettingValue = string.IsNullOrWhiteSpace(value) ? "0" : value,
                    ModifiedTime = DateTime.Now
                };
                Services.GetService<PersonalSettingsService>().Create(ps);
            }
            else
            {
                _ps.SettingValue = string.IsNullOrWhiteSpace(value) ? "0" : value;
                _ps.ModifiedTime = DateTime.Now;
                Services.GetService<PersonalSettingsService>().Update(_ps);
            }

        }
    }
}