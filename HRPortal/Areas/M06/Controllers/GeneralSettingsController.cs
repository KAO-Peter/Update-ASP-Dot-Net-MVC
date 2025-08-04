using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.DBEntities;
using HRPortal.Services;
using System.Resources;
using System.Reflection;

namespace HRPortal.Areas.M06.Controllers
{
    public class GeneralSettingsController : BaseController
    {
        //
        // GET: /M06/GeneralSettings/
        public ActionResult Index()
        {
            List<SystemSettingViewModel> _model = new List<SystemSettingViewModel>();
            IEnumerable<SystemSetting> _settings = this.Services.GetService<SystemSettingService>().GetAll();

            ResourceManager _resourceManager = new ResourceManager("HRPortal.MultiLanguage.Resource", typeof(HRPortal.MultiLanguage.Resource).Assembly);

            foreach (SystemSetting _setting in _settings)
            {
                _model.Add(new SystemSettingViewModel()
                {
                    DisplayName = _resourceManager.GetString(_setting.SettingKey) ?? _setting.SettingKey,
                    SettingKey = _setting.SettingKey,
                    SettingValue = _setting.SettingValue,
                });
            }
            return View(_model);
        }

        [HttpPost]
        public ActionResult Save(List<SystemSettingViewModel> model)
        {
            SystemSettingService _service = Services.GetService<SystemSettingService>();

            foreach(SystemSettingViewModel _setting in model)
            {
                if(_service.UpdateSetting(_setting.SettingKey, _setting.SettingValue) != 0)
                {
                    WriteLog(string.Format("{0} = {1}", _setting.SettingKey, _setting.SettingValue));
                }
            }

            return RedirectToAction("Index");
        }
    }
}