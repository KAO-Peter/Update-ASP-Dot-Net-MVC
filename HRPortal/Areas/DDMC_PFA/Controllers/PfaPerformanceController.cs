using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaPerformanceController : BaseController
    {
        #region Index
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtCode = "", string txtName = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtCode, txtName);

            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaPerformanceService>().GetPfaPerformanceData(txtCode, txtName);
            var viewModel = ds.Select(x => new PfaPerformanceViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                Code = x.Code,
                Name = x.Name,
                Performance = x.Performance,
                band = x.band,
                Rates = x.Rates,
                Multiplier = x.Multiplier,
                Scores = x.ScoresStart + "~" + x.ScoresEnd
            }).ToPagedList(currentPage, currentPageSize);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtCode, string txtName, string btnQuery, string btnClear)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                return RedirectToAction("Index", new
                {
                    page = 1,
                    txtCode,
                    txtName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCode, txtName);
            return View();
        }

        private void GetDefaultData(string txtCompanyID = "", string txtCode = "", string txtName = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }
            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtCode = txtCode;
            ViewBag.txtName = txtName;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            CreatePfaPerformance model = new CreatePfaPerformance();
            model.ID = Guid.Empty;
            model.CompanyID = CurrentUser.CompanyID;
            return PartialView("_CreatePfaPerformance", model);
        }

        [HttpPost]
        public ActionResult Create(CreatePfaPerformance model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var isExist = Services.GetService<PfaPerformanceService>().IsExist(model.Code);
                if (isExist)
                {
                    WriteLog(string.Format("評等區間代碼已存在,Code:{0}", model.Code));
                    return Json(new { success = false, message = "評等區間代碼已存在" });
                }

                var IsOverLapping = Services.GetService<PfaPerformanceService>().IsOverLapping(model.ID, model.ScoresStart, model.ScoresEnd);
                if (IsOverLapping)
                {
                    WriteLog(string.Format("分數區間與其他評等重疊請檢查"));
                    return Json(new { success = false, message = "分數區間與其他評等重疊請檢查" });
                }

                PfaPerformance data = new PfaPerformance();
                data.ID = Guid.NewGuid();
                data.Code = model.Code;
                data.Name = model.Name;
                data.IsUsed = true;
                data.Performance = model.Performance;
                data.band = model.band;
                data.Rates = model.Rates;
                data.Multiplier = model.Multiplier;
                data.ScoresStart = model.ScoresStart;
                data.ScoresEnd = model.ScoresEnd;
                data.CompanyID = model.CompanyID;
                data.CreatedBy = CurrentUser.EmployeeID;
                data.CreatedTime = DateTime.Now;

                int IsSuccess = Services.GetService<PfaPerformanceService>().Create(data);
                if (IsSuccess == 1)
                {
                    WriteLog(string.Format("新增成功,ID:{0}", data.ID));
                    return Json(new { success = true, message = "新增成功" });
                }
            }
            return Json(new { success = true, message = "新增成功" });
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? id)
        {
            PfaPerformance data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaPerformanceService>().GetPfaPerformance(id);
            }

            CreatePfaPerformance model = new CreatePfaPerformance();
            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.Code = data.Code;
                model.Name = data.Name;
                model.Performance = data.Performance;
                model.band = data.band;
                model.Rates = data.Rates;
                model.Multiplier = data.Multiplier;
                model.ScoresStart = data.ScoresStart;
                model.ScoresEnd = data.ScoresEnd;
            }
            return PartialView("_EditPfaPerformance", model);
        }

        [HttpPost]
        public ActionResult Edit(CreatePfaPerformance model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var data = Services.GetService<PfaPerformanceService>().GetPfaPerformance(model.ID);
                if (data == null)
                {
                    WriteLog(string.Format("查無此評等區間,ID:{0}", model.ID));
                    return Json(new { success = false, message = "查無此評等區間" });
                }
                else
                {
                    var IsOverLapping = Services.GetService<PfaPerformanceService>().IsOverLapping(model.ID, model.ScoresStart, model.ScoresEnd);
                    if (IsOverLapping)
                    {
                        WriteLog(string.Format("分數區間與其他評等重疊請檢查"));
                        return Json(new { success = false, message = "分數區間與其他評等重疊請檢查" });
                    }

                    //data.Name = model.Name;
                    data.Rates = model.Rates;
                    //data.Multiplier = model.Multiplier;
                    //data.ScoresStart = model.ScoresStart;
                    //data.ScoresEnd = model.ScoresEnd;
                    data.CompanyID = model.CompanyID;
                    data.ModifiedBy = CurrentUser.EmployeeID;
                    data.ModifiedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaPerformanceService>().Update(data);
                    if (IsSuccess == 1)
                    {
                        WriteLog(string.Format("編輯成功,ID:{0}", data.ID));
                        return Json(new { success = true, message = "編輯成功" });
                    }
                }
            }
            return Json(new { success = true, message = "編輯成功" });
        }
        #endregion

        #region Delete
        public ActionResult Delete(Guid? id)
        {
            PfaPerformance data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaPerformanceService>().GetPfaPerformance(id);
            }

            CreatePfaPerformance model = new CreatePfaPerformance();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.Code = data.Code;
                model.Name = data.Name;
                model.Performance = data.Performance;
                model.band = data.band;
                model.Rates = data.Rates;
                model.Multiplier = data.Multiplier;
                model.ScoresStart = data.ScoresStart;
                model.ScoresEnd = data.ScoresEnd;
            }
            return PartialView("_DeletePfaPerformance", model);
        }

        [HttpPost]
        public ActionResult Delete(CreatePfaPerformance model)
        {
            Result result = null;

            PfaPerformance data = Services.GetService<PfaPerformanceService>().GetPfaPerformance(model.ID);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此績效考核組織";
                result.log = "查無此績效考核組織";
            }
            else
            {
                int IsSuccess = Services.GetService<PfaPerformanceService>().Delete(data);
                if (IsSuccess == 1)
                {
                    WriteLog(string.Format("刪除成功"));
                    return Json(new { success = true, message = "刪除成功" });
                }
            }
            return Json(new { success = true, message = "刪除成功" });
        }
        #endregion
    }
}