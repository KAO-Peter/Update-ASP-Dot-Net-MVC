using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using System.IO;
using HRPortal.DBEntities;

namespace HRPortal.Controllers
{
    public class DownloadFilesController : BaseController
    {
        //
        // GET: /DownloadFiles/
        public ActionResult Index(int page = 1, string keyword = "")
        {
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<DownloadFilesService>().GetDownloadFileList(keyword);
            ViewBag.Keyword = keyword;

            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int page = 1, string txtkeyword = "", string searchName = "")
        {
            ViewBag.Keyword = txtkeyword;
            if (searchName == "1" && txtkeyword != null && txtkeyword != "篩選標題")
            {
                page = 1;
              
                return RedirectToAction("Index", new
                {
                    page = page,
                    keyword = txtkeyword
                });
            }
           
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<DownloadFilesService>().GetDownloadFileList(txtkeyword);

            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        //2017.03.16增加 by Daniel，顯示公告附件內容會用到
        public FileStreamResult DownloadStream(Guid id)
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("DownloadFilePath"));
            DownloadFile _fileData = Services.GetService<DownloadFilesService>().FirstOrDefault(x => x.Id == id);
            using (FileStream _file = new FileStream(Path.Combine(_path, _fileData.Path), FileMode.Open))
            {
                byte[] buffer = new byte[16 * 1024];
                //因為File物件會自動加上content-disposition為attachment，所以改用FileStrameResult
                //FileStreamResult會自動關閉，此處MemoryStream不需要用Using (用Using反而會提前關閉Stream)
                MemoryStream _ms = new MemoryStream();

                int _read;
                while ((_read = _file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _ms.Write(buffer, 0, _read);
                }
                _ms.Position = 0;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.AddHeader("Content-Disposition", "inline;filename=" + _fileData.OriginalName);

                return new FileStreamResult(_ms, _fileData.Format);

            }
        }

        public ActionResult Download(Guid id)
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("DownloadFilePath"));
            DownloadFile _fileData = Services.GetService<DownloadFilesService>().FirstOrDefault(x => x.Id == id);
            using (FileStream _file = new FileStream(Path.Combine(_path, _fileData.Path), FileMode.Open))
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
                    return File(_ms.ToArray(), _fileData.Format, _fileData.OriginalName);
                }
            }
        }
    }
}