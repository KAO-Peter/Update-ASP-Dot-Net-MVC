using HRPortal.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class SignFlowLevelSettingController : BaseController
    {
        // GET: ToDo/SignFlowLevelSetting
        public ActionResult Index()
        {
            SignFlowLevelRepository _repository = new SignFlowLevelRepository();
            List<SignFlowLevel> _model = _repository.GetIsUsedSignFlowLevel().ToList();
            return View(_model);
        }

        public ActionResult Create()
        {
            return PartialView("_CreateSignFlowLevel");
        }

        [HttpPost]
        public ActionResult Create(SignFlowLevel model)
        {
            model.IsUsed = "Y";

            SignFlowLevelRepository _repository = new SignFlowLevelRepository();

            try
            {
                _repository.Create(model);
                _repository.SaveChanges();
                TempData["message"] = "新增成功";
                WriteLog("Success:" + model.LevelID);
                return Json(new { success = true });
            }
            catch
            {
                TempData["message"] = "新增失敗";
                return PartialView("_CreateSignFlowLevel", model);
            }
        }

        public ActionResult Edit(string id)
        {
            SignFlowLevelRepository _repository = new SignFlowLevelRepository();
            SignFlowLevel _model = _repository.GetByPrimaryKeys(new object[] { id });
            return PartialView("_EditSignFlowLevel", _model);
        }

        [HttpPost]
        public ActionResult Edit(SignFlowLevel model)
        {
            model.IsUsed = "Y";

            SignFlowLevelRepository _repository = new SignFlowLevelRepository();

            try
            {
                _repository.Update(model);
                _repository.SaveChanges();
                TempData["message"] = "變更成功";
                WriteLog("Success:" + model.LevelID);
                return Json(new { success = true });
            }
            catch
            {
                TempData["message"] = "變更失敗";
                return PartialView("_EditSignFlowLevel", model);
            }
        }
    }
}