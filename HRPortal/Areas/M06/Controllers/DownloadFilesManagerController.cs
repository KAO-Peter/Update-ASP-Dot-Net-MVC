using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Services;
using HRPortal.Mvc.Controllers;
using PagedList;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using System.IO;

namespace HRPortal.Areas.M06.Controllers
{
    public class DownloadFilesManagerController : BaseController
    {
        // GET: M06/DownloadFilesManager
        public ActionResult Index(int page = 1, string keyword = "")
        {
            SetDefault();
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<DownloadFilesService>().GetDownloadFileList(keyword);
            ViewBag.Keyword = keyword;

            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string deletedId, int page = 1, string txtkeyword = "", string searchName = "")
        {
            ViewBag.Keyword = txtkeyword;
            //更新狀態
            if (deletedId != null && deletedId != "0")
            {
                Services.GetService<DownloadFilesService>().DeleteFile(Guid.Parse(deletedId), CurrentUser.Employee.ID, true);
                TempData["message"] = "成功刪除";
                WriteLog("Delete " + deletedId);
                page = 1;
            }
            else if (searchName == "1" && txtkeyword != null && txtkeyword != "篩選標題")
            {
                page = 1;

                return RedirectToAction("Index", new
                {
                    page = page,
                    keyword = txtkeyword
                });
            }
            SetDefault();
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<DownloadFilesService>().GetDownloadFileList(txtkeyword);

            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        public ActionResult Create(string fileOringinName, string filePathName, string fileSize, string fileFormat)
        {
            SetDefault();
            DownloadFilesViewModel viewmodel = new DownloadFilesViewModel();
            DownloadFile model = new DownloadFile();
            model.IsDeleted = false;
            model.Createdby = CurrentUser.Employee.ID;
            model.Path = filePathName;
            model.OriginalName = HttpUtility.UrlDecode(fileOringinName);
            model.Size = int.Parse(fileSize);
            model.Format = fileFormat;
            viewmodel.DownloadFileData = model;
            return PartialView("_CreateFile", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateDownloadFile(DownloadFilesViewModel model)
        {
            SetDefault();
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateFile", model);
            }
            else
            {
                int IsSuccess = Services.GetService<DownloadFilesService>().Create(model.DownloadFileData, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "新增成功";
                    WriteLog("Success:"+ model.DownloadFileData.Id);
                    return Json(new { success = true });
                }
            }
            return PartialView("_CreateFile", model);
        }

        public ActionResult Edit(string Id)
        {
            SetDefault();
            DownloadFilesViewModel viewmodel = new DownloadFilesViewModel();
            DownloadFile model = new DownloadFile();
            if (!string.IsNullOrWhiteSpace(Id))
               viewmodel.DownloadFileData = Services.GetService<DownloadFilesService>().GetDownloadFileByID(Guid.Parse(Id));
            return PartialView("_EditFile", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditDownloadFile(DownloadFilesViewModel model)
        {
            SetDefault();
            if (!ModelState.IsValid)
            {
                return PartialView("_EditFile", model);
            }
            else
            {
                model.DownloadFileData.Modifiedby = CurrentUser.Employee.ID;
                int IsSuccess = Services.GetService<DownloadFilesService>().UpdateFile(model.DownloadFileData, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "編輯成功";
                    WriteLog("Success:"+ model.DownloadFileData.Id);
                    return Json(new { success = true });
                }
            }
            
            return PartialView("_EditFile", model);
        }
        

        [HttpPost]
        public virtual ActionResult UploadFile()
        {
            string filePathName = "";
            int fileSize = 0;
            string fileFormat = "";
            string fileOringinName = "";
            //## 如果有任何檔案類型才做
            if (Request.Files.AllKeys.Any())
            {
                //## 讀取指定的上傳檔案ID
                var httpPostedFile = Request.Files["UploadedFile"];

                //## 真實有檔案，進行上傳
                if (httpPostedFile != null && httpPostedFile.ContentLength != 0 && httpPostedFile.ContentLength <= 100000000)
                {
                    filePathName = Services.GetService<DownloadFilesService>().UploadFile(httpPostedFile);
                    fileFormat = httpPostedFile.ContentType;
                    fileSize = httpPostedFile.ContentLength;
                    fileOringinName = httpPostedFile.FileName;
                    WriteLog("Upload:" + fileOringinName+"/"+filePathName+"/"+fileFormat+"/"+fileSize);

                }
                else 
                {
                    //100mb= 100000000 
                    TempData["message"] = "檔案大小限制為100MB";
                    return Json(new { success = false});

                }
            }
            return Json(new { success = true, fileOringinName = HttpUtility.UrlEncode(fileOringinName), filePathName, fileFormat, fileSize });
        }

        private void SetDefault()
        {
            SetBaseUserInfo();
        }

        public ActionResult Download(string filePath, string fileFormat, string fileName)
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("DownloadFilePath"));
            using (FileStream _file = new FileStream(Path.Combine(_path, filePath), FileMode.Open))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream _ms = new MemoryStream())
                {
                    int _read;
                    while ((_read = _file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _ms.Write(buffer, 0, _read);
                    }
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return File(_ms.ToArray(), fileFormat, fileName);
                }
            }
        }
    }
}