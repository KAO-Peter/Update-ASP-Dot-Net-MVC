using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M06.Controllers
{
    public class OptionSettingsController : BaseController
    {
        //
        // GET: /M06/OptionSettings/
        public ActionResult Index()
        {
            List<OptionGroup> _groups = Services.GetService<OptionGroupService>().GetAll().OrderBy(x => x.OptionGroupKey).ToList();
            List<SelectListItem> _listItem = new List<SelectListItem>();
            _listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.DropDownListSelect, Value = "", Selected = true });
            foreach(OptionGroup _group in _groups)
            {
                _listItem.Add(new SelectListItem { Text = _group.OptionGroupName, Value = _group.OptionGroupKey });
            }
            ViewData["GroupList"] = _listItem;

            return View();
        }

        public ActionResult CreateGroup()
        {
            return PartialView("_CreateGroup");
        }

        [HttpPost]
        public ActionResult CreateGroup(OptionGroup model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                TempData["message"] = "驗證失敗請檢查頁面資料";
                return PartialView("_CreateGroup", model);
            }
            else
            {
                model.ID = Guid.NewGuid();
                model.CreatedBy = CurrentUser.EmployeeID;
                model.CreatedTime = DateTime.Now;

                int IsSuccess = Services.GetService<OptionGroupService>().Create(model);
                if (IsSuccess == 1)
                {
                    TempData["GroupKey"] = model.OptionGroupKey;
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + model.ID);
                    return Json(new { success = true });
                }
            }

            TempData["message"] = "新增成功";
            return PartialView("_CreateGroup", model);
        }

        public ActionResult LoadOptionList(string groupKey)
        {
            IList<Option> _model = Services.GetService<OptionService>().GetOptionListByGroup(groupKey);
            return PartialView("_OptionList", _model);
        }

        public ActionResult EditOption(Guid? id)
        {
            if (id.HasValue)
            {
                Option _model = Services.GetService<OptionService>().FirstOrDefault(x => x.ID == id.Value);
                return PartialView("_EditOption", _model);
            }
            else
                return PartialView("_EditOption");
        }

        [HttpPost]
        public ActionResult DeleteOption(Guid optionId)
        {
            Option _model = Services.GetService<OptionService>().FirstOrDefault(x => x.ID == optionId);
            if (_model != null)
            {
                try
                {
                    TempData["GroupKey"] = _model.OptionGroup.OptionGroupKey;
                    Services.GetService<OptionService>().Delete(_model);
                    TempData["message"] = "刪除成功";
                    WriteLog("Success:" + optionId);

                    return Json(new AjaxResult() { status = "success", message = "刪除成功" });
                }
                catch
                {
                    return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
                }
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "無資料" });
            }
        }

        [HttpPost]
        public ActionResult SaveOption(Option model)
        {
            int IsSuccess;

            if (model.ID == Guid.Empty)
            {
                model.ID = Guid.NewGuid();

                OptionGroup _group = Services.GetService<OptionGroupService>().FirstOrDefault(x => x.OptionGroupKey == model.OptionGroup.OptionGroupKey);
                model.OptionGroup = _group;
                model.OptionGroupID = _group.ID;

                model.CreatedBy = CurrentUser.EmployeeID;
                model.CreatedTime = DateTime.Now;

                IsSuccess = Services.GetService<OptionService>().Create(model);
            }
            else
            {
                Option _option = Services.GetService<OptionService>().FirstOrDefault(x => x.ID == model.ID);
                _option.OptionValue = model.OptionValue;
                _option.DisplayResourceName = model.DisplayResourceName;
                _option.DisplayName = model.DisplayName;
                _option.ModifiedBy = CurrentUser.EmployeeID;
                _option.ModifiedTime = DateTime.Now;

                IsSuccess = Services.GetService<OptionService>().Update(_option);
            }

            if (IsSuccess == 1)
            {
                TempData["GroupKey"] = model.OptionGroup.OptionGroupKey;
                TempData["message"] = "儲存成功";
                WriteLog("Success:" + model.ID);
                return Json(new { success = true });
            }

            return PartialView("_EditOption", model);
        }
	}
}