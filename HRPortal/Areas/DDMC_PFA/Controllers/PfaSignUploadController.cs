using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using HRPortal.DBEntities.DDMC_PFA;
using System.Collections.Generic;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    /// <summary>
    /// 績效考核簽核附件
    /// </summary>
    public class PfaSignUploadController : BaseController
    {
        /// <summary>
        /// 檔案筆數
        /// </summary>
        /// <param name="PfaCycleEmpID"></param>
        /// <returns></returns>
        public ActionResult QuerySignUpload(string PfaCycleEmpID)
        {
            try
            {
                if (!string.IsNullOrEmpty(PfaCycleEmpID))
                {
                    Guid pfaCycleEmpID = Guid.Parse(PfaCycleEmpID);

                    var pfaSignUploadCount = Services.GetService<PfaSignUploadService>().GetAll().Where(x => x.PfaCycleEmpID == pfaCycleEmpID).Count();

                    return Json(new { success = true, message = "", data = pfaSignUploadCount });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = true, message = "", data = 0 });
        }

        #region 檔案上傳
        public ActionResult Upload(string pfaCycleEmpId, string pfaSignProcessId, string cmd)
        {
            Guid? PfaCycleEmpID = null;
            if (!string.IsNullOrEmpty(pfaCycleEmpId))
                PfaCycleEmpID = Guid.Parse(pfaCycleEmpId);

            Guid? PfaSignProcessID = null;
            if (!string.IsNullOrEmpty(pfaSignProcessId))
                PfaSignProcessID = Guid.Parse(pfaSignProcessId);

            ViewBag.pfaCycleEmpId = pfaCycleEmpId;
            ViewBag.pfaSignProcessId = pfaSignProcessId;
            ViewBag.cmd = cmd;
            ViewBag.filename = "";

            var pfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.ID == PfaSignProcessID);
            if (pfaSignProcess == null)
                return View("_Upload");

            var result = new PfaSignUploadDataViewModel
            {
                IsUpload = pfaSignProcess.IsUpload
            };

            result.PfaSignUpload = Services.GetService<PfaSignUploadService>().GetAll()
                                                                              .Where(x => x.PfaCycleEmpID == PfaCycleEmpID).OrderBy(x => x.Ordering)
                                                                              .ToList().Select(x =>
                                                                              {
                                                                                  return new PfaSignUploadViewModel
                                                                                  {
                                                                                      ID = x.ID,
                                                                                      PfaCycleEmpID = x.PfaCycleEmpID,
                                                                                      PfaSignProcessID = x.PfaCycleEmpID,
                                                                                      FileName = x.FileName,
                                                                                      Href = x.Href,
                                                                                  };
                                                                              }).ToList();

            return View("_Upload", result);
        }

        [HttpPost]
        public ActionResult Upload(string pfaCycleEmpId, string pfaSignProcessId, string filename, HttpPostedFileBase file)
        {
            var result = new Result { success = true };

            if (file == null)
            {
                WriteLog("請選擇要上傳的檔案");
                return Json(new { success = false, message = "請選擇要上傳的檔案" });
            }

            try
            {
                Guid? PfaCycleEmpID = null;
                if (!string.IsNullOrEmpty(pfaCycleEmpId))
                    PfaCycleEmpID = Guid.Parse(pfaCycleEmpId);

                Guid? PfaSignProcessID = null;
                if (!string.IsNullOrEmpty(pfaSignProcessId))
                    PfaSignProcessID = Guid.Parse(pfaSignProcessId);

                if (string.IsNullOrWhiteSpace(filename))
                    filename = "其他說明";

                if (!PfaSignProcessID.HasValue)
                {
                    WriteLog("查無簽核資料，上傳檔案錯誤");
                    return Json(new { success = false, message = "查無簽核資料，上傳檔案錯誤" });
                }

                if (file.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() != ".pdf")
                {
                    WriteLog("檔案格式不支援，應為PDF");
                    return Json(new { success = false, message = "檔案格式不支援，應為PDF" });
                }

                var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.ID == PfaCycleEmpID);
                if (pfaCycleEmp == null)
                    throw new Exception("績效考核員工資料取得失敗");

                var companyCode = pfaCycleEmp.Employees.Company.CompanyCode;
                if (string.IsNullOrWhiteSpace(companyCode))
                {
                    WriteLog("查無公司別資料");
                    return Json(new { success = false, message = "查無公司別資料" });
                }

                var employeeNO = pfaCycleEmp.Employees.EmployeeNO;

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaCycleEmp.PfaCycleID);
                if (pfaCycle == null)
                    throw new Exception("績效考核資料取得失敗");

                var pfaFormNo = pfaCycle.PfaFormNo;
                if (string.IsNullOrWhiteSpace(pfaFormNo))
                {
                    WriteLog("查無績效考核批號資料");
                    return Json(new { success = false, message = "查無績效考核批號資料" });
                }

                //附件上傳於 公司別/表單類別/Attached 資料夾存放。
                string dirPath = Server.MapPath(string.Format("~/Areas/DDMC_PFA/Attached/{0}/{1}/{2}", companyCode, pfaFormNo, employeeNO));
                string searchDirPath = string.Format("\\Areas\\DDMC_PFA\\Attached\\{0}\\{1}\\{2}\\", companyCode, pfaFormNo, employeeNO);
                int seq = 0;

                var pfaSignUpload = Services.GetService<PfaSignUploadService>().GetAll().Where(x => x.PfaCycleEmpID == PfaCycleEmpID && x.Href.Contains(searchDirPath)).OrderByDescending(x => x.Href).FirstOrDefault();
                if (pfaSignUpload == null)
                    seq = 1;
                else
                {
                    string[] nowFileName = Path.GetFileNameWithoutExtension(pfaSignUpload.Href).Split('_');
                    seq = int.Parse(nowFileName[1]) + 1;
                }

                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

                if (file.ContentLength > 0)
                {
                    string fileExtension = Path.GetExtension(file.FileName);
                    //檔案名稱 => 考核批號_序號2碼
                    string newFileName = string.Format("{0}_{1}{2}", pfaFormNo, seq.ToString().PadLeft(2, '0'), fileExtension);

                    string href = Path.Combine(dirPath, newFileName);
                    file.SaveAs(href);

                    var newPfaSignUploadList = new List<PfaSignUpload>();
                    var newPfaSignUpload = new PfaSignUpload
                    {
                        ID = Guid.NewGuid(),
                        PfaSignProcessID = PfaSignProcessID.Value,
                        PfaCycleEmpID = PfaCycleEmpID.Value,
                        Ordering = seq,
                        FileName = filename,
                        Href = href,
                        CreatedBy = CurrentUser.EmployeeID,
                        CreatedTime = DateTime.Now,
                    };
                    newPfaSignUploadList.Add(newPfaSignUpload);
                    result = Services.GetService<PfaSignUploadService>().CreatePfaSignUpload(newPfaSignUploadList);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("上傳失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "上傳失敗" });
            }
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public ActionResult DelFile(string pfaSignUploadId)
        {
            Guid? PfaSignUploadId = null;
            if (!string.IsNullOrEmpty(pfaSignUploadId))
                PfaSignUploadId = Guid.Parse(pfaSignUploadId);

            var deleteData = Services.GetService<PfaSignUploadService>().Where(x => x.ID == PfaSignUploadId).Any();
            if (deleteData)
            {
                Result result = Services.GetService<PfaSignUploadService>().DelPfaSignUpload(PfaSignUploadId.Value);

                WriteLog(result.log);
                return Json(new { success = result.success, message = result.message });
            }
            else
            {
                WriteLog("查無附件資料");
                return Json(new { success = false, message = "查無附件資料" });
            }
        }

        public FileResult Download(string pfaSignUploadId)
        {
            try
            {
                Guid? PfaSignUploadId = null;
                if (!string.IsNullOrEmpty(pfaSignUploadId))
                    PfaSignUploadId = Guid.Parse(pfaSignUploadId);

                var data = Services.GetService<PfaSignUploadService>().Where(x => x.ID == PfaSignUploadId).FirstOrDefault();
                if (data == null)
                    throw new Exception("查無績效考核簽核附件");

                string filepath = data.Href;
                string tempfilename = System.IO.Path.GetFileName(filepath);
                string[] tempFilenameArr = tempfilename.Split('.');
                string filename = string.Format("{0}.{1}", data.FileName, tempFilenameArr[1]);
                Stream iStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(iStream, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("error pfaSignUploadId={0}, msg={1}", pfaSignUploadId, ex.Message));
                string viewpdfexceptionfilepath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/view_pdf_exception.pdf");
                byte[] data = FileToBytes(viewpdfexceptionfilepath);

                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                return File(data, "application/pdf");
            }
        }
        #endregion

        #region 載入附件檔案
        /// <summary>
        /// 載入附件檔案
        /// </summary>
        /// <param name="pfaSignUploadId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LoadPfaSignUpload(string pfaSignUploadId)
        {
            var attach = GetPfaSignUpload(pfaSignUploadId);
            if (attach != null)
            {
                try
                {
                    byte[] data = AttachToBytes(attach);
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return File(data, "application/pdf");
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("error pfaSignUploadId={0}, msg={1}", pfaSignUploadId, ex.Message));
                    string viewpdfexceptionfilepath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/view_pdf_exception.pdf");
                    byte[] data = FileToBytes(viewpdfexceptionfilepath);

                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return File(data, "application/pdf");
                }
            }
            return View();
        }

        /// <summary>
        /// 取得附件檔案物件資訊
        /// </summary>
        /// <param name="pfaSignUploadId"></param>
        /// <returns></returns>
        private PfaSignUpload GetPfaSignUpload(string pfaSignUploadId)
        {
            Guid? PfaSignUploadID = null;
            if (!string.IsNullOrEmpty(pfaSignUploadId))
                PfaSignUploadID = Guid.Parse(pfaSignUploadId);
            return Services.GetService<PfaSignUploadService>().Where(x => x.ID == PfaSignUploadID).FirstOrDefault();
        }

        /// <summary>
        /// 附件檔案轉二進位陣列。
        /// </summary>
        /// <param name="attach"></param>
        /// <returns></returns>
        private byte[] AttachToBytes(PfaSignUpload attach)
        {
            return FileToBytes(attach.Href);
        }

        /// <summary>
        /// 檔案轉二進位陣列
        /// </summary>
        /// <param name="filepath">檔案路徑</param>
        /// <returns></returns>
        private byte[] FileToBytes(string filepath)
        {
            byte[] bytes;

            using (FileStream fsSource = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;
            }
            return bytes;
        }
        #endregion
    }
}