using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class SignFlowFormLevelSettingController : BaseController
    {
        //
        // GET: /M06/OptionSettings/
        public ActionResult Index()
        {
            OptionListHelper _opionListHelp = new OptionListHelper();
            ViewData["FormTypeList"] = _opionListHelp.GetOptionList("FormType").Where(x => !x.Value.ToLower().Contains("cancel")).ToList();

            return View();
        }

        public ActionResult LoadFormLevelList(string formType)
        {
            SignFlowFormLevelRepository _repository = new SignFlowFormLevelRepository();
            IList<SignFlowFormLevel> _model = _repository.GetSignFlowFormLevelByFormType(formType);
            return PartialView("_FormLevelList", _model);
        }

        public ActionResult AddFormLevel()
        {
            SignFlowLevelRepository _signFlowRepository = new SignFlowLevelRepository();
            IList<SignFlowLevel> _signFlowList = _signFlowRepository.GetIsUsedSignFlowLevel().ToList();
            List<SelectListItem> _signFlowListItems = new List<SelectListItem>();
            foreach (SignFlowLevel _signFlow in _signFlowList)
            {
                _signFlowListItems.Add(new SelectListItem() { Text = _signFlow.Name, Value = _signFlow.LevelID });
            }
            ViewData["SignFlowLevelList"] = _signFlowListItems;
            return PartialView("_AddFormLevel");
        }

        [HttpPost]
        public ActionResult AddFormLevel(SignFlowFormLevel model)
        {
            TempData["FormType"] = model.FormType;

            SignFlowFormLevelRepository _repository = new SignFlowFormLevelRepository();
            if (_repository.GetSignFlowFormLevelByFormType(model.FormType).Any(x => x.LevelID == model.LevelID))
            {
                TempData["message"] = "層級已存在";
                return Json(new { success = false });
            }
            else
            {
                SignFlowSeqRepository _seqRepository = new SignFlowSeqRepository();
                model.FormLevelID = _seqRepository.GetSignFromLevelSeq();
                model.IsUsed = "Y";

                try
                {
                    _repository.Create(model);
                    _repository.SaveChanges();
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + model.FormLevelID);
                    return Json(new { success = true });
                }
                catch
                {
                    TempData["message"] = "新增失敗";
                    return Json(new { success = false });
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteFormLevel(string formLevelId)
        {
            SignFlowFormLevelRepository _repository = new SignFlowFormLevelRepository();
            SignFlowFormLevel _formLevel = _repository.GetAll().FirstOrDefault(x => x.FormLevelID == formLevelId);
            if (_formLevel == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "層級不存在" });
            }
            else
            {
                SignFlowDesignRepository _designRepository = new SignFlowDesignRepository();
                if(_designRepository.GetAll().Any(x => x.IsUsed == "Y" && x.FormLevelID == formLevelId))
                {
                    return Json(new AjaxResult() { status = "failed", message = "層級仍在使用中,無法刪除" });
                }

                _formLevel.IsUsed = "N";
                try
                {
                    _repository.Update(_formLevel);
                    _repository.SaveChanges();
                    TempData["message"] = "刪除成功";
                    TempData["FormType"] = _formLevel.FormType;
                    WriteLog("Success:" + formLevelId);

                    return Json(new AjaxResult() { status = "success", message = "刪除成功" });
                }
                catch
                {
                    return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
                }
            }
        }
    }
}