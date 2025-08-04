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
    
    public class ReadTodayManagerController : BaseController
    {
        //int pageSize = 3;
        //
        // GET: /M06/ReadTodayManager/
        public ActionResult Index(string Status = "",int page=1, string keyword = "")
        {
            SetInfo(Status,keyword);
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<ReadTodayService>().GetAnnounceAllList(Status, keyword);
           
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  ActionResult Index(
            string changeId, string selectChangeStatus, string SearchTitle, string txtkeyword, int page = 1)
        {
            //更新狀態
            if (changeId != null && changeId != "0" && changeId.IndexOf("S/") >= 0)
            {
                //上架
                Services.GetService<ReadTodayService>().UpdateStatus(true, Guid.Parse(changeId.Replace("S/", "")), CurrentUser.Employee.ID);
                TempData["message"] = "上架成功";
                WriteLog("Status to show.(" + changeId.Replace("S/", "")+")");
            }
            else if (changeId != null && changeId != "0" && changeId.IndexOf("H/") >= 0)
            {
                //下架
                Services.GetService<ReadTodayService>().UpdateStatus(false, Guid.Parse(changeId.Replace("H/", "")), CurrentUser.Employee.ID);
                TempData["message"] = "下架成功";
                WriteLog("Status to hide.(" + changeId.Replace("H/", "") + ")");
            }
            else if (!string.IsNullOrWhiteSpace(selectChangeStatus) || (SearchTitle == "1" && txtkeyword!=null && txtkeyword!= "篩選標題"))
            {
                page = 1;
               
                return RedirectToAction("Index", new
                {
                    page = page,
                    keyword = txtkeyword,
                    Status = selectChangeStatus
                });
            }
            //重整
            SetInfo();
            int currentPage = page < 1 ? 1 : page;
            //ViewData["StatusList"] = new SelectList(GetSelectShow(), "Value", "Text", 0);
            var ds = Services.GetService<ReadTodayService>().GetAnnounceAllList(selectChangeStatus, "");
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        public ActionResult Create()
        {
            SetInfo();
            ReadTodayViewModel ViewModel = new ReadTodayViewModel();
            ReadToday model = new ReadToday();
            model.Status = true;
            model.Createdby = CurrentUser.Employee.ID;
            model.AnnounceStartTime = DateTime.Now;
            if (model.AnnounceEndTime != null)
            {
                ViewModel.IsShowEndTime = true;
            }
            else
            {
                ViewModel.IsShowEndTime = false;
                model.AnnounceEndTime = DateTime.Now;
            }
            ViewModel.AnnouncementData = model;
            ViewModel.FilesLists = GetFilesList();
            ViewModel.AnnouncementData.IsSticky = false;
            return PartialView("_CreateAnnounce", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(ReadTodayViewModel model)
        {
            SetInfo();
            if (!ModelState.IsValid)
            {
                //WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_CreateAnnounce",model);
            }
            else
            {
                if (model.IsShowEndTime == false)
                {
                    model.AnnouncementData.AnnounceEndTime = null;
                }
                else if (model.IsShowEndTime == true && model.AnnouncementData.AnnounceEndTime == null)
                {
                    TempData["message"] = "請輸入結束日期";
                    return PartialView("_CreateAnnounce",model);
                }
                model.AnnouncementData.Id = Guid.NewGuid();

                int IsSuccess = Services.GetService<ReadTodayService>().Create(model.AnnouncementData, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "新增成功";
                    //新增檔案
                    if (!string.IsNullOrWhiteSpace(model.FileGuid))
                    {
                        ReadTodayFiles filedata = new ReadTodayFiles();
                        filedata.AnnouncementID = model.AnnouncementData.Id;
                        filedata.DownloadFilesID =Guid.Parse(model.FileGuid);
                        var _fileresult = Services.GetService<ReadTodayFilesService>().Create(filedata);
                    }
                   // ModelState.Clear();
                    WriteLog("Success:" + model.AnnouncementData.Id);
                    return Json(new { success = true });
                }
            }
                return PartialView("_CreateAnnounce",model);
        }

        public ActionResult Edit(Guid id)
        {
            SetInfo();
            ReadTodayViewModel ViewModel = new ReadTodayViewModel();
            ReadToday model = new ReadToday();
            model = Services.GetService<ReadTodayService>().GetAnnounceByID(id);
            model.Modifiedby = CurrentUser.Employee.ID;
            if (model.AnnounceEndTime != null)
            {
                ViewModel.IsShowEndTime = true;
            }
            else
            {
                ViewModel.IsShowEndTime = false;
                //model.AnnounceEndTime = DateTime.Now;
            }
            ViewModel.AnnouncementData = model;
            //判斷是否有檔案
            var filedata = Services.GetService<ReadTodayFilesService>().GetFileIDByAnnouncement(model.Id);
            string fileID = "";
            if (filedata != null && filedata.Id!=Guid.Empty)
                fileID = filedata.Id.ToString();
            ViewModel.FilesLists = GetFilesList(fileID);
            return PartialView("_EditAnnounce", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(ReadTodayViewModel model)
        {

            SetInfo();
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_EditAnnounce", model);
            }
            else
            {
                if (model.IsShowEndTime == false)
                {
                    model.AnnouncementData.AnnounceEndTime = null;
                }
                else if (model.IsShowEndTime == true && model.AnnouncementData.AnnounceEndTime == null)
                {
                    TempData["message"] = "請輸入結束日期";
                    return PartialView("_EditAnnounce", model);
                }
                ReadToday olddata = Services.GetService<ReadTodayService>().GetAnnounceByID(model.AnnouncementData.Id);

                int IsSuccess = Services.GetService<ReadTodayService>().Update(olddata, model.AnnouncementData, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "修改成功";
                    //新增檔案
                    if (!string.IsNullOrWhiteSpace(model.FileGuid))
                    {
                        ReadTodayFiles filedata = new ReadTodayFiles();
                        filedata.AnnouncementID = model.AnnouncementData.Id;
                        filedata.DownloadFilesID = Guid.Parse(model.FileGuid);
                        var _fileresult = Services.GetService<ReadTodayFilesService>().Create(filedata);
                    }
                    WriteLog("Success:" + model.AnnouncementData.Id);
                    return Json(new { success = true });
                }
            }
            return PartialView("_EditAnnounce", model);
        }

        private void SetInfo(string statusdata="",string txtkeyword = "")
        {
            SetBaseUserInfo();
            ViewData["StatusList"] = GetSelectShow(statusdata);
            ViewBag.StatusData = statusdata;
            ViewBag.TitleKeyword = txtkeyword;

        }

        /// <summary>
        /// 上下架選單
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSelectShow(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.AnnouncementStatusShow, Value = ((int)AnnouncementStatus.Show).ToString(), Selected = (selecteddata == ((int)AnnouncementStatus.Show).ToString() ? true : false) });
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.AnnouncementStatusHide, Value = ((int)AnnouncementStatus.Hide).ToString(), Selected = (selecteddata == ((int)AnnouncementStatus.Hide).ToString() ? true : false) });
            return listItem;
        }

        /// <summary>
        /// 取得檔案列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetFilesList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<DownloadFile> data = Services.GetService<DownloadFilesService>().GetDownloadFileAllList().ToList();
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            listItem.Add(new SelectListItem { Text = "無", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString(), Selected = (SelectedDataID == item.Id ? true : false) });
            }
            return listItem;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deleted(string DeletedID)
        {
            ReadToday olddata = Services.GetService<ReadTodayService>().GetAnnounceByID(Guid.Parse(DeletedID));
            int _result = Services.GetService<ReadTodayService>().Delete(olddata, olddata, CurrentUser.EmployeeID, true);
            if (_result == 1)
            {
                WriteLog("Success:" + DeletedID);
                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            else
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
        }


    }
}