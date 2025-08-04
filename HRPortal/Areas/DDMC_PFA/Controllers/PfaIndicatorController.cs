using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaIndicatorController : BaseController
    {
        #region Index
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtPfaIndicatorCode = "", string txtPfaIndicatorName = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtPfaIndicatorCode, txtPfaIndicatorName);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaIndicatorService>().GetPfaIndicatorData(string.IsNullOrWhiteSpace(txtCompanyID) ? Guid.Empty : Guid.Parse(txtCompanyID), txtPfaIndicatorCode, txtPfaIndicatorName);

            var viewModel = ds.Select(x => new PfaIndicatorViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                PfaIndicatorCode = x.PfaIndicatorCode,
                PfaIndicatorName = x.PfaIndicatorName,
                Description = x.Description,
                Scale = x.Scale,
                Ordering = x.Ordering,
            }).ToPagedList(currentPage, currentPageSize);

            foreach (var item in viewModel)
            {
                item.PfaIndicatorDetail = Services.GetService<PfaIndicatorDetailService>().GetAll().Where(y => y.PfaIndicatorID == item.ID).OrderBy(y => y.Ordering)
                                                .Select(y => new PfaIndicatorDetailViewModel() { Ordering = y.Ordering, PfaIndicatorKey = y.PfaIndicatorKey, UpperLimit = y.UpperLimit, LowerLimit = y.LowerLimit }).ToList();
            }

            return View(viewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        /// <param name="btnQuery"></param>
        /// <param name="btnClear"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtCompanyID, string txtPfaIndicatorCode, string txtPfaIndicatorName, string btnQuery, string btnClear)
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
                    txtCompanyID,
                    txtPfaIndicatorCode,
                    txtPfaIndicatorName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtPfaIndicatorCode, txtPfaIndicatorName);

            return View();
        }

        /// <summary>
        /// 取得預設值
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        private void GetDefaultData(string txtCompanyID = "", string txtPfaIndicatorCode = "", string txtPfaIndicatorName = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }
            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtPfaIndicatorCode = txtPfaIndicatorCode;
            ViewBag.txtPfaIndicatorName = txtPfaIndicatorName;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            PfaIndicatorCreateViewModel model = new PfaIndicatorCreateViewModel();
            model.ID = Guid.Empty;
            return PartialView("_CreatePfaIndicator", model);
        }

        [HttpPost]
        public ActionResult Create(PfaIndicatorCreateViewModel obj)
        {
            if (obj.Data.Length == 0)
            {
                WriteLog("請輸入程度說明");
                return Json(new { success = false, message = "請輸入程度說明" });
            }
            else
            {
                var IsRepeat = Services.GetService<PfaIndicatorService>().IsRepeat(obj.PfaIndicatorCode);
                if (IsRepeat)
                {
                    WriteLog(string.Format("指標代碼已存在,PfaIndicatorCode:{0}", obj.ID));
                    return Json(new { success = false, message = "指標代碼已存在" });
                }

                if (obj.Data != null)
                {
                    foreach(var item in obj.Data)
                    {
                        if (!Regex.IsMatch(item.UpperLimit.Value.ToString(), @"^[0-9]+(.[0-9]{1})?$"))
                        {
                            WriteLog(string.Format("上限僅能輸至小數點後一位,PfaIndicatorCode:{0}", item.ID));
                            return Json(new { success = false, message = "上限僅能輸至小數點後一位" });
                        }
                        if (!Regex.IsMatch(item.UpperLimit.Value.ToString(), @"^[0-9]+(.[0-9]{1})?$"))
                        {
                            WriteLog(string.Format("下限僅能輸至小數點後一位,PfaIndicatorCode:{0}", item.ID));
                            return Json(new { success = false, message = "下限僅能輸至小數點後一位" });
                        }
                    }
                }

                PfaIndicator PfaIndicator = new PfaIndicator();
                PfaIndicator.ID = Guid.NewGuid();
                PfaIndicator.PfaIndicatorCode = obj.PfaIndicatorCode;
                PfaIndicator.PfaIndicatorName = obj.PfaIndicatorName;
                PfaIndicator.Description = obj.Description;
                PfaIndicator.Scale = obj.Scale;
                PfaIndicator.IsUsed = true;
                PfaIndicator.Ordering = obj.Ordering;
                PfaIndicator.CompanyID = CurrentUser.CompanyID;
                PfaIndicator.CreatedBy = CurrentUser.EmployeeID;
                PfaIndicator.CreatedTime = DateTime.Now;

                List<PfaIndicatorDetail> PfaIndicatorDetail = new List<PfaIndicatorDetail>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaIndicatorDetail flow = new PfaIndicatorDetail();
                    flow.ID = Guid.NewGuid();
                    flow.PfaIndicatorID = PfaIndicator.ID;
                    flow.PfaIndicatorKey = item.PfaIndicatorKey;
                    flow.UpperLimit = item.UpperLimit;
                    flow.LowerLimit = item.LowerLimit;
                    flow.Ordering = item.Ordering;
                    flow.CreatedBy = CurrentUser.EmployeeID;
                    flow.CreatedTime = DateTime.Now;
                    PfaIndicatorDetail.Add(flow);
                }

                if (PfaIndicator != null && PfaIndicatorDetail.Count > 0)
                {
                    Result result = Services.GetService<PfaIndicatorService>().CreatePfaIndicator(PfaIndicator, PfaIndicatorDetail);
                    WriteLog(result.log);
                    return Json(new { success = result.success, message = result.message });
                }
                else
                {
                    WriteLog("請輸入簽核步驟");
                    return Json(new { success = false, message = "請輸入簽核步驟" });
                }
            }
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? id)
        {
            if (id.HasValue)
            {
                PfaIndicatorCreateViewModel model = new PfaIndicatorCreateViewModel();

                PfaIndicator data = Services.GetService<PfaIndicatorService>().GetPfaIndicator(id);
                if (data == null)
                {
                    return PartialView("_EditPfaIndicator");
                }
                else
                {
                    model.ID = id.Value;
                    model.Ordering = data.Ordering;
                    model.PfaIndicatorCode = data.PfaIndicatorCode;
                    model.PfaIndicatorName = data.PfaIndicatorName;
                    model.Scale = data.Scale;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaIndicatorDetailService>().GetIndicatorDetail(id);

                    model.Data = new PfaIndicatorDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaIndicatorDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaIndicatorID = Detail[i].PfaIndicatorID;
                        model.Data[i].PfaIndicatorKey = Detail[i].PfaIndicatorKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                        model.Data[i].Ordering = Detail[i].Ordering;
                    }
                    return PartialView("_EditPfaIndicator", model);
                }
            }
            else
            {
                return PartialView("_EditPfaIndicator");
            }
        }

        [HttpPost]
        public ActionResult Edit(PfaIndicatorCreateViewModel obj)
        {
            if (obj.Data.Length == 0)
            {
                WriteLog("請輸入程度說明");
                return Json(new { success = false, message = "請輸入程度說明" });
            }
            else
            {
                var IsExits = Services.GetService<PfaIndicatorService>().GetAll().Where(x => x.ID != obj.ID && x.PfaIndicatorCode == obj.PfaIndicatorCode).Any();
                if (IsExits)
                {
                    WriteLog(string.Format("指標代碼已存在,PfaIndicatorCode:{0}", obj.ID));
                    return Json(new { success = false, message = "指標代碼已存在" });
                }

                if (obj.Data != null)
                {
                    foreach (var item in obj.Data)
                    {
                        if (!Regex.IsMatch(item.UpperLimit.Value.ToString(), @"^[0-9]+(.[0-9]{1})?$"))
                        {
                            WriteLog(string.Format("上限僅能輸至小數點後一位,PfaIndicatorCode:{0}", item.ID));
                            return Json(new { success = false, message = "上限僅能輸至小數點後一位" });
                        }
                        if (!Regex.IsMatch(item.UpperLimit.Value.ToString(), @"^[0-9]+(.[0-9]{1})?$"))
                        {
                            WriteLog(string.Format("下限僅能輸至小數點後一位,PfaIndicatorCode:{0}", item.ID));
                            return Json(new { success = false, message = "下限僅能輸至小數點後一位" });
                        }
                    }
                }

                PfaIndicator PfaIndicator = new PfaIndicator();
                PfaIndicator.ID = obj.ID;
                PfaIndicator.PfaIndicatorCode = obj.PfaIndicatorCode;
                PfaIndicator.PfaIndicatorName = obj.PfaIndicatorName;
                PfaIndicator.Description = obj.Description;
                PfaIndicator.Scale = obj.Scale;
                PfaIndicator.IsUsed = true;
                PfaIndicator.Ordering = obj.Ordering;
                PfaIndicator.CompanyID = CurrentUser.CompanyID;
                PfaIndicator.ModifiedBy = CurrentUser.EmployeeID;
                PfaIndicator.ModifiedTime = DateTime.Now;

                List<PfaIndicatorDetail> PfaIndicatorDetail = new List<PfaIndicatorDetail>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaIndicatorDetail flow = new PfaIndicatorDetail();
                    flow.ID = Guid.NewGuid();
                    flow.PfaIndicatorID = obj.ID;
                    flow.PfaIndicatorKey = item.PfaIndicatorKey;
                    flow.UpperLimit = item.UpperLimit;
                    flow.LowerLimit = item.LowerLimit;
                    flow.Ordering = item.Ordering;
                    flow.CreatedBy = CurrentUser.EmployeeID;
                    flow.CreatedTime = DateTime.Now;
                    PfaIndicatorDetail.Add(flow);
                }

                if (PfaIndicator != null && PfaIndicatorDetail.Count > 0)
                {
                    Result result = Services.GetService<PfaIndicatorService>().EditPfaIndicator(PfaIndicator, PfaIndicatorDetail);
                    WriteLog(result.log);
                    return Json(new { success = result.success, message = result.message });
                }
                else
                {
                    WriteLog("請選擇指標代碼");
                    return Json(new { success = false, message = "請選擇指標代碼" });
                }
            }
        }
        #endregion

        #region Detail
        public ActionResult Detail(Guid? id)
        {
            if (id.HasValue)
            {
                PfaIndicatorCreateViewModel model = new PfaIndicatorCreateViewModel();

                PfaIndicator data = Services.GetService<PfaIndicatorService>().GetPfaIndicator(id);
                if (data == null)
                {
                    return PartialView("_DetailPfaIndicator");
                }
                else
                {
                    model.ID = id.Value;
                    model.Ordering = data.Ordering;
                    model.PfaIndicatorCode = data.PfaIndicatorCode;
                    model.PfaIndicatorName = data.PfaIndicatorName;
                    model.Scale = data.Scale;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaIndicatorDetailService>().GetIndicatorDetail(id);

                    model.Data = new PfaIndicatorDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaIndicatorDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaIndicatorID = Detail[i].PfaIndicatorID;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].PfaIndicatorKey = Detail[i].PfaIndicatorKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                    }
                    return PartialView("_DetailPfaIndicator", model);
                }
            }
            else
            {
                return PartialView("_DetailPfaIndicator");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(Guid? id)
        {
            if (id.HasValue)
            {
                PfaIndicatorCreateViewModel model = new PfaIndicatorCreateViewModel();

                PfaIndicator data = Services.GetService<PfaIndicatorService>().GetPfaIndicator(id);
                if (data == null)
                {
                    return PartialView("_DeletePfaIndicator");
                }
                else
                {
                    model.ID = id.Value;
                    model.Ordering = data.Ordering;
                    model.PfaIndicatorCode = data.PfaIndicatorCode;
                    model.PfaIndicatorName = data.PfaIndicatorName;
                    model.Scale = data.Scale;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaIndicatorDetailService>().GetIndicatorDetail(id);

                    model.Data = new PfaIndicatorDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaIndicatorDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaIndicatorID = Detail[i].PfaIndicatorID;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].PfaIndicatorKey = Detail[i].PfaIndicatorKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                    }
                    return PartialView("_DeletePfaIndicator", model);
                }
            }
            else
            {
                return PartialView("_DeletePfaIndicator");
            }
        }

        [HttpPost]
        public ActionResult DeletePfaIndicator(Guid? id)
        {
            Result result = Services.GetService<PfaIndicatorService>().DeletePfaIndicator(id.Value);
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion
    }
}