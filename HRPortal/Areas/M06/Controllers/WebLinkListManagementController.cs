using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M06.Controllers
{
    public class WebLinkListManagementController : BaseController
    {
        // GET: /M06/WebLinkListManagement/
        public ActionResult Index(string deletedId, string changeId)
        {
           
            if (deletedId != null && deletedId != "0")
            {
                Services.GetService<WebLinkListService>().DeleteLink(Guid.Parse(deletedId), CurrentUser.Employee.ID, true);
                TempData["message"] = "成功刪除";
                WriteLog("Delete " + deletedId);
            }
            else if (changeId != null && changeId != "0") {
                WebLinkList _link = Services.GetService<WebLinkListService>().GetLinkByID(Guid.Parse(changeId));
                Services.GetService<WebLinkListService>().UpdateStatus((_link.IsUsed == true ? false : true), Guid.Parse(changeId), CurrentUser.Employee.ID);
            }
           
            List<WebLinkList> _model = Services.GetService<WebLinkListService>().GetAllLinkList().OrderBy(x => x.Number).ThenBy(y=>y.ID).ToList();
            return View(_model);
        }

        public ActionResult Create()
        {
            return PartialView("_CreateLink");
        }

        [HttpPost]
        public ActionResult Create(HomeWebLinkModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_CreateLink", model);
            }
            else
            {


                WebLinkList _link = new WebLinkList()
                {
                    ID = Guid.NewGuid(),
                    WebName = model.Name,
                    WebLink = model.URL,
                    Modifiedby=CurrentUser.Employee.ID,
                    Createdby = CurrentUser.Employee.ID,
                    IsUsed = model.IsUsed,
                    Number=model.Number
                };

                int IsSuccess = Services.GetService<WebLinkListService>().Create(_link);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + _link.ID);
                    return Json(new { success = true });
                }
            }
            return PartialView("_CreateLink", model);
        }

        public ActionResult Edit(Guid id)
        {
            WebLinkList _link = Services.GetService<WebLinkListService>().FirstOrDefault(x => x.ID == id);

            HomeWebLinkModel _model = new HomeWebLinkModel()
            {
                ID = _link.ID,
                Number=_link.Number,
                Name = _link.WebName,
                URL = _link.WebLink,
                IsUsed=_link.IsUsed
            };

            return PartialView("_EditLink", _model);
        }


        [HttpPost]
        public ActionResult Edit(HomeWebLinkModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_EditLink", model);
            }
            else
            {
                Dictionary<string, bool> _roleParams = new Dictionary<string, bool>();

                WebLinkList _link = Services.GetService<WebLinkListService>().FirstOrDefault(x => x.ID == model.ID);
                _link.WebName = model.Name;
                _link.WebLink = model.URL;
                _link.Modifiedby=CurrentUser.Employee.ID;
                _link.Number = model.Number;
                _link.IsUsed = model.IsUsed;

                int IsSuccess = Services.GetService<WebLinkListService>().Update(_link);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "修改成功";
                    WriteLog("Success:" + model.ID);
                    return Json(new { success = true });
                }
            }
            return PartialView("_EditLink", model);
        }
    }
}
