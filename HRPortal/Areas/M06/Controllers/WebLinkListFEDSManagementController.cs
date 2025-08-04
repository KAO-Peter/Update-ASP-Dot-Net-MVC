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
    public class WebLinkListFEDSManagementController : BaseController
    {
        // GET: /M06/WebLinkListFEDSManagement/
        public ActionResult Index(string deletedId, string changeId)
        {

            if (deletedId != null && deletedId != "0")
            {
                Services.GetService<WebLinkListFEDSService>().DeleteLink(Guid.Parse(deletedId), CurrentUser.Employee.ID, true);
                TempData["message"] = "成功刪除";
                WriteLog("Delete " + deletedId);
            }
            else if (changeId != null && changeId != "0")
            {
                WebLinkListFEDS _link = Services.GetService<WebLinkListFEDSService>().GetLinkByID(Guid.Parse(changeId));
                Services.GetService<WebLinkListFEDSService>().UpdateStatus((_link.IsUsed == true ? false : true), Guid.Parse(changeId), CurrentUser.Employee.ID);
            }

            List<WebLinkListFEDS> _model = Services.GetService<WebLinkListFEDSService>().GetAllLinkList().OrderBy(x => x.Number).ThenBy(y => y.ID).ToList();
            
            return View(_model);
        }

        public ActionResult Create()
        {
            HomeWebLinkFEDSModel viewmodel = new HomeWebLinkFEDSModel();

            viewmodel.webLists = GetwebList("");
            return PartialView("_CreateLink", viewmodel);
        }
        /// <summary>
        /// 取得選單列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetwebList(string selecteddata)
        {
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            List<OptionGroup> _groups = Services.GetService<OptionGroupService>().GetAll().Where(x =>  x.OptionGroupKey == "weblist").ToList();
            List<SelectListItem> _listItem = new List<SelectListItem>();
            foreach (var _group in _groups)
            {
                foreach(var i in _group.Options.OrderBy(x=>x.CreatedTime))
                {
                    _listItem.Add(new SelectListItem { Text = i.DisplayName, Value =i.OptionValue, Selected = (SelectedDataID == _group.ID ? true : false) });
                }
            }            
            return _listItem;
        }
        [HttpPost]
        public ActionResult Create(HomeWebLinkFEDSModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_CreateLink", model);
            }
            else
            {

                //取OptionID
                var _OptionID = Services.GetService<OptionService>().GetAll().Where(x => x.OptionValue == model.CurrentOptionGroupKey).Select(x => x.ID).FirstOrDefault();
                   
                WebLinkListFEDS _link = new WebLinkListFEDS()
                {
                    ID = Guid.NewGuid(),
                    WebName = model.Name,
                    WebLink = model.URL,
                    Modifiedby = CurrentUser.Employee.ID,
                    Createdby = CurrentUser.Employee.ID,
                    IsUsed = model.IsUsed,
                    Number = model.Number,
                    OptionID = _OptionID,
                    
                };

                int IsSuccess = Services.GetService<WebLinkListFEDSService>().Create(_link);
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
            WebLinkListFEDS _link = Services.GetService<WebLinkListFEDSService>().FirstOrDefault(x => x.ID == id);
            var _OptionName = Services.GetService<OptionService>().GetAll().Where(x => x.ID == _link.OptionID).Select(x => x.DisplayName).FirstOrDefault();
            HomeWebLinkFEDSModel _model = new HomeWebLinkFEDSModel()
            {
                ID = _link.ID,
                Number = _link.Number,
                Name = _link.WebName,
                URL = _link.WebLink,
                IsUsed = _link.IsUsed,
                OptionName = _OptionName,
            };
            
            return PartialView("_EditLink", _model);
        }


        [HttpPost]
        public ActionResult Edit(HomeWebLinkFEDSModel model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_EditLink", model);
            }
            else
            {
                Dictionary<string, bool> _roleParams = new Dictionary<string, bool>();

                WebLinkListFEDS _link = Services.GetService<WebLinkListFEDSService>().FirstOrDefault(x => x.ID == model.ID);
                _link.WebName = model.Name;
                _link.WebLink = model.URL;
                _link.Modifiedby = CurrentUser.Employee.ID;
                _link.Number = model.Number;
                _link.IsUsed = model.IsUsed;

                int IsSuccess = Services.GetService<WebLinkListFEDSService>().Update(_link);
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
