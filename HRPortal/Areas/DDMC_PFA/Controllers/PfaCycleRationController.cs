using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaCycleRationController : BaseController
    {
        #region Index
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtCycleID = "", string txtOrgID = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtCycleID, txtOrgID);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaCycleRationService>().GetData(string.IsNullOrWhiteSpace(txtCompanyID) ? Guid.Empty : Guid.Parse(txtCompanyID), string.IsNullOrWhiteSpace(txtCycleID) ? Guid.Empty : Guid.Parse(txtCycleID), string.IsNullOrWhiteSpace(txtOrgID) ? Guid.Empty : Guid.Parse(txtOrgID));

            var viewModel = ds.Select(x => new PfaCycleRationViewModel()
            {
                ID = x.ID,
                PfaCycleID = x.PfaCycleID,
                PfaFormNo = x.PfaCycle.PfaFormNo,
                PfaOrgID = x.PfaOrgID,
                PfaOrgName = x.PfaOrg.PfaOrgName,
                OrgTotal = x.OrgTotal,
                Status = x.PfaCycle.Status
            }).ToPagedList(currentPage, currentPageSize);

            var PerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.IsUsed).ToList();
            foreach (var item in viewModel)
            {
                item.RationDetail = PerformanceList.Select(y => new PfaCycleRationTitleViewModel() { ID = y.ID, Code = y.Code, Name = y.Name, Ordering = y.Ordering }).OrderBy(y => y.Ordering).ThenBy(y => y.Code).ToList();

                foreach (var detail in item.RationDetail)
                {
                    var CycleRationDetail = Services.GetService<PfaCycleRationDetailService>().GetAll().Where(y => y.PfaCycleRationID == item.ID && y.PfaPerformanceID == detail.ID).FirstOrDefault();
                    if (CycleRationDetail != null)
                    {
                        detail.Staffing = CycleRationDetail.Staffing;
                    }
                }
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
        public ActionResult Index(string txtCompanyID, string txtCycleID, string txtOrgID, string btnQuery, string btnClear)
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
                    txtCycleID,
                    txtOrgID,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtCycleID, txtOrgID);

            return View();
        }

        /// <summary>
        /// 取得預設值
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        private void GetDefaultData(string txtCompanyID = "", string txtCycleID = "", string txtOrgID = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }

            List<PfaCycleRationTitleViewModel> TitleItem = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.IsUsed).OrderBy(x => x.Ordering).ThenBy(x => x.Code).Select(x => new PfaCycleRationTitleViewModel { Name = x.Name }).ToList();

            ViewData["CycleList"] = GetCycleList(txtCycleID);
            ViewData["OrgList"] = GetOrgList(txtOrgID);
            ViewData["TitleItem"] = TitleItem;

            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtCycleID = txtCycleID;
            ViewBag.txtOrgID = txtOrgID;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();
            model.ID = Guid.Empty;
            model.CycleList = GetCycleList(string.Empty, "c");
            model.OrgList = new List<SelectListItem>
            {
               new SelectListItem { Text = "請選擇", Value = "", Selected = true}
            };
            return PartialView("_CreatePfaCycleRation", model);
        }

        [HttpPost]
        public ActionResult Create(PfaCycleRationCreateViewModel obj)
        {
            if (obj.PfaCycleID == null || obj.PfaCycleID == Guid.Empty)
            {
                WriteLog("請選擇考核批號");
                return Json(new { success = false, message = "請選擇身份類別" });
            }
            if (obj.PfaOrgID == null || obj.PfaOrgID == Guid.Empty)
            {
                WriteLog("請選擇部門類別");
                return Json(new { success = false, message = "請選擇部門類別" });
            }

            var isExist = Services.GetService<PfaCycleRationService>().GetAll().Where(x => x.PfaCycleID == obj.PfaCycleID && x.PfaOrgID == obj.PfaOrgID).Any();
            if (isExist)
            {
                WriteLog(string.Format("資料已存在"));
                return Json(new { success = false, message = "資料已存在" });
            }

            PfaCycleRation PfaCycleRation = new PfaCycleRation();
            PfaCycleRation.ID = Guid.NewGuid();
            PfaCycleRation.PfaCycleID = obj.PfaCycleID;
            PfaCycleRation.PfaOrgID = obj.PfaOrgID;
            PfaCycleRation.OrgTotal = obj.OrgTotal;
            PfaCycleRation.CreatedBy = CurrentUser.EmployeeID;
            PfaCycleRation.CreatedTime = DateTime.Now;

            List<PfaCycleRationDetail> PfaCycleRationDetail = new List<PfaCycleRationDetail>();

            int idx = 0;

            foreach (var item in obj.Data)
            {
                idx += 1;
                PfaCycleRationDetail flow = new PfaCycleRationDetail();
                flow.ID = Guid.NewGuid();
                flow.PfaCycleRationID = PfaCycleRation.ID;
                flow.PfaPerformanceID = item.PfaPerformanceID;
                flow.Staffing = item.Staffing;
                flow.CreatedBy = CurrentUser.EmployeeID;
                flow.CreatedTime = DateTime.Now;
                PfaCycleRationDetail.Add(flow);
            }

            if (PfaCycleRation != null && PfaCycleRationDetail.Count > 0)
            {
                var PfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.IsUsed).OrderBy(x => x.Ordering).ThenBy(x => x.Code).ToList();

                foreach (var item in PfaCycleRationDetail)
                {
                    var Performance = PfaPerformance.Where(x => x.ID == item.PfaPerformanceID).FirstOrDefault();
                    if (Performance != null)
                    {
                        item.Code = Performance.Code;
                        item.Name = Performance.Name;
                        item.Ordering = Performance.Ordering;
                        item.Performance = Performance.Performance;
                        item.band = Performance.band;
                        item.Rates = Performance.Rates;
                        item.Multiplier = Performance.Multiplier;
                        item.ScoresStart = Performance.ScoresStart;
                        item.ScoresEnd = Performance.ScoresEnd;
                    }

                    double BudgetTotal = Convert.ToDouble(PfaCycleRation.OrgTotal * item.Rates * item.Multiplier / 100);
                    BudgetTotal = BudgetTotal * 100;
                    BudgetTotal = Math.Floor(BudgetTotal);
                    BudgetTotal = BudgetTotal / 100;

                    item.BudgetTotal = Convert.ToDecimal(BudgetTotal.ToString("#0.0"));
                    item.TotalScore = item.Multiplier * item.Staffing;
                }

                PfaCycleRation.TotalScore = PfaCycleRationDetail.Select(x => x.TotalScore).Sum();

                if (PfaCycleRation.OrgTotal < PfaCycleRation.TotalScore)
                {
                    WriteLog("核對試算總數不得大於總人數，請再調整");
                    return Json(new { success = false, message = "核對試算總數不得大於總人數，請再調整" });
                }

                Result result = Services.GetService<PfaCycleRationService>().CreatePfaCycleRation(PfaCycleRation, PfaCycleRationDetail);
                WriteLog(result.log);
                return Json(new { success = result.success, message = result.message });
            }
            else
            {
                WriteLog("請輸入簽核步驟");
                return Json(new { success = false, message = "請輸入簽核步驟" });
            }
        }

        public ActionResult QueryDetail(string PfaCycleID, string PfaOrgID)
        {
            PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();
            try
            {
                if (!string.IsNullOrEmpty(PfaCycleID) && !string.IsNullOrEmpty(PfaOrgID))
                {
                    Guid cid = Guid.Parse(PfaCycleID);
                    Guid oid = Guid.Parse(PfaOrgID);

                    var PfaOrg = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrgID == oid).Select(x => x.PfaDept.PfaDeptName).ToList();

                    string str = "";
                    foreach (var dept in PfaOrg)
                    {
                        if (!string.IsNullOrEmpty(str)) str += "、";
                        str += dept;
                    }

                    var count = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == cid && x.PfaOrgID == oid && x.IsRatio).Count();

                    model.PfaCycleID = cid;
                    model.PfaOrgID = oid;
                    model.CycleList = GetCycleList(PfaCycleID, "c");
                    model.OrgList = GetOrgList(PfaOrgID, cid);
                    model.DeptName = str;
                    model.OrgTotal = count;

                    var Detail = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.IsUsed).OrderBy(x => x.Ordering).ThenBy(x => x.Code).ToList();

                    model.Data = new PfaCycleRationDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaCycleRationDetailCreateViewModel();
                        model.Data[i].PfaCycleRationID = cid;
                        model.Data[i].PfaPerformanceID = Detail[i].ID;
                        model.Data[i].Code = Detail[i].Code;
                        model.Data[i].Name = Detail[i].Name;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].Performance = Detail[i].Performance;
                        model.Data[i].band = Detail[i].band;
                        model.Data[i].Rates = Detail[i].Rates;
                        model.Data[i].Multiplier = Detail[i].Multiplier;
                        model.Data[i].ScoresStart = Detail[i].ScoresStart;
                        model.Data[i].ScoresEnd = Detail[i].ScoresEnd;
                        model.Data[i].Total = count;

                        double BudgetTotal = Convert.ToDouble(count * Detail[i].Rates * Detail[i].Multiplier / 100);
                        BudgetTotal = BudgetTotal * 100;
                        BudgetTotal = Math.Floor(BudgetTotal);
                        BudgetTotal = BudgetTotal / 100;

                        model.Data[i].BudgetTotal = Convert.ToDecimal(BudgetTotal.ToString("#0.0"));
                    }

                    StringBuilder dataResult = new StringBuilder();
                    foreach (var item in model.Data)
                    {
                        if (!item.Staffing.HasValue)
                            item.Staffing = 0;
                        dataResult.Append(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr'>"));
                        dataResult.Append(string.Format("<input id='CycleRationDetailID' name='CycleRationDetailID' type='hidden' value='{0}>", item.ID));
                        dataResult.Append(string.Format("<input id='PfaCycleRationID' name='PfaCycleRationID' type='hidden' value='{0}'>", item.PfaCycleRationID));
                        dataResult.Append(string.Format("<input id='PfaPerformanceID' name='PfaPerformanceID' type='hidden' value='{0}'>", item.PfaPerformanceID));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Performance));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Rates));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.band));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Multiplier));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Total));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.BudgetTotal));
                        dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'><input class='form-control' id='Staffing' name='Staffing' type='text' value='{0}'></td>", item.Staffing));
                        dataResult.Append(string.Format("<td id='CalStaffing' name='CalStaffing' style='white-space:normal;text-align:center;'>{0}</td>", item.TotalScore));
                        dataResult.Append("</tr>");
                    }

                    StringBuilder dataEnd = new StringBuilder();
                    dataEnd.Append(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr'>"));
                    dataEnd.Append(string.Format("<td style='white-space:normal;text-align:right;' colspan='6'>{0}</td>", "合計"));
                    dataEnd.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}　<button type='button' id='btnCal' class='btn btn-primary' style:'float:right'>試算</button></td>", ""));
                    dataEnd.Append(string.Format("<td id='ScoreTotal' name='ScoreTotal' style='white-space:normal;text-align:center;'>{0}</td>", ""));
                    dataEnd.Append("</tr>");

                    return Json(new { success = true, message = "", detail = model, data = dataResult.ToString(), dataEnd = dataEnd.ToString() });
                }
                else
                {
                    WriteLog("考核批號和部門類別不可空白");
                    return Json(new { success = false, message = "考核批號和部門類別不可空白", detail = model, data = string.Empty });
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("取得組織應配比人數失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "取得組織應配比人數失敗", detail = model, data = string.Empty });
            }
        }

        public ActionResult GetOrgTotal(string PfaCycleID, string PfaOrgID)
        {
            try
            {
                if (!string.IsNullOrEmpty(PfaCycleID) && !string.IsNullOrEmpty(PfaOrgID))
                {
                    Guid cid = Guid.Parse(PfaCycleID);
                    Guid oid = Guid.Parse(PfaOrgID);

                    OrgChangeItem result = new OrgChangeItem();

                    var PfaOrg = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrgID == oid).Select(x => x.PfaDept.PfaDeptName).ToList();

                    string str = "";
                    foreach (var dept in PfaOrg)
                    {
                        if (!string.IsNullOrEmpty(str)) str += "、";
                        str += dept;
                    }

                    var count = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == cid && x.PfaOrgID == oid && x.IsRatio).Count();

                    result.DeptName = str;
                    result.Total = count;
                    return Json(new { success = true, changeitem = result });
                }
                else
                {
                    WriteLog("考核批號和部門類別不可空白");
                    return Json(new { success = false, message = "考核批號和部門類別不可空白", changeitem = string.Empty });
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("取得組織應配比人數失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "取得組織應配比人數失敗", changeitem = string.Empty });
            }
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? id)
        {
            if (id.HasValue)
            {
                PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();

                PfaCycleRation data = Services.GetService<PfaCycleRationService>().GetPfaCycleRation(id);
                if (data == null)
                {
                    return PartialView("_EditPfaCycleRation");
                }
                else
                {
                    model.ID = id.Value;
                    model.PfaCycleID = data.PfaCycleID;
                    model.CycleList = GetCycleList(data.PfaCycleID.ToString());
                    model.PfaOrgID = data.PfaOrgID;
                    model.OrgList = GetOrgList(data.PfaOrgID.ToString(), data.PfaCycleID);
                    model.OrgTotal = data.OrgTotal;

                    var PfaOrg = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrgID == data.PfaOrgID).Select(x => x.PfaDept.PfaDeptName).ToList();
                    string str = "";
                    foreach (var dept in PfaOrg)
                    {
                        if (!string.IsNullOrEmpty(str)) str += "、";
                        str += dept;
                    }
                    model.DeptName = str;

                    var Detail = Services.GetService<PfaCycleRationDetailService>().GetPfaCycleRationDetail(id);

                    model.StaffingTotal = Detail.Select(x => x.Staffing).Sum();
                    model.ScoreTotal = Detail.Select(x => x.TotalScore).Sum();

                    model.Data = new PfaCycleRationDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaCycleRationDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaCycleRationID = Detail[i].PfaCycleRationID;
                        model.Data[i].PfaPerformanceID = Detail[i].PfaPerformanceID;
                        model.Data[i].Code = Detail[i].Code;
                        model.Data[i].Name = Detail[i].Name;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].Performance = Detail[i].Performance;
                        model.Data[i].band = Detail[i].band;
                        model.Data[i].Rates = Detail[i].Rates;
                        model.Data[i].Multiplier = Detail[i].Multiplier;
                        model.Data[i].Total = data.OrgTotal.Value;
                        model.Data[i].BudgetTotal = Detail[i].BudgetTotal;
                        model.Data[i].Staffing = Detail[i].Staffing;
                        model.Data[i].TotalScore = Detail[i].TotalScore;
                    }
                    return PartialView("_EditPfaCycleRation", model);
                }
            }
            else
            {
                return PartialView("_EditPfaCycleRation");
            }
        }

        [HttpPost]
        public ActionResult Edit(PfaCycleRationCreateViewModel obj)
        {
            if (obj.PfaCycleID == null || obj.PfaCycleID == Guid.Empty)
            {
                WriteLog("請選擇績效考核批號");
                return Json(new { success = false, message = "請選擇績效考核批號" });
            }
            else if (obj.Data.Length == 0)
            {
                WriteLog("請輸入關鍵行為指標");
                return Json(new { success = false, message = "請輸入關鍵行為指標" });
            }
            else
            {
                PfaCycleRation PfaCycleRation = new PfaCycleRation();
                PfaCycleRation.ID = obj.ID;
                PfaCycleRation.PfaCycleID = obj.PfaCycleID;
                PfaCycleRation.PfaOrgID = obj.PfaOrgID;
                PfaCycleRation.OrgTotal = obj.OrgTotal;
                PfaCycleRation.ModifiedBy = CurrentUser.EmployeeID;
                PfaCycleRation.ModifiedTime = DateTime.Now;

                var Detail = Services.GetService<PfaCycleRationDetailService>().GetPfaCycleRationDetail(obj.ID);

                List<PfaCycleRationDetail> PfaCycleRationDetail = new List<PfaCycleRationDetail>();

                foreach (var item in obj.Data)
                {
                    var UpdateDetail = Detail.Where(x => x.ID == item.ID).FirstOrDefault();
                    if (UpdateDetail != null)
                    {
                        UpdateDetail.Staffing = item.Staffing;
                        UpdateDetail.TotalScore = item.Staffing * UpdateDetail.Multiplier;
                        UpdateDetail.ModifiedBy = CurrentUser.EmployeeID;
                        UpdateDetail.ModifiedTime = DateTime.Now;
                    }

                    PfaCycleRationDetail.Add(UpdateDetail);
                }

                PfaCycleRation.TotalScore = PfaCycleRationDetail.Select(x => x.TotalScore).Sum();

                if (PfaCycleRation.OrgTotal < PfaCycleRation.TotalScore)
                {
                    WriteLog("核對試算總數不得大於總人數，請再調整");
                    return Json(new { success = false, message = "核對試算總數不得大於總人數，請再調整" });
                }

                if (PfaCycleRation != null && PfaCycleRationDetail.Count > 0)
                {
                    Result result = Services.GetService<PfaCycleRationService>().EditPfaCycleRation(PfaCycleRation, PfaCycleRationDetail);
                    WriteLog(result.log);
                    return Json(new { success = result.success, message = result.message });
                }
                else
                {
                    WriteLog("請選擇身份類別");
                    return Json(new { success = false, message = "請選擇身份類別" });
                }
            }
        }
        #endregion

        #region Detail
        public ActionResult Detail(Guid? id)
        {
            if (id.HasValue)
            {
                PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();

                PfaCycleRation data = Services.GetService<PfaCycleRationService>().GetPfaCycleRation(id);
                if (data == null)
                {
                    return PartialView("_DetailPfaCycleRation");
                }
                else
                {
                    model.ID = id.Value;
                    model.PfaCycleID = data.PfaCycleID;
                    model.CycleList = GetCycleList(data.PfaCycleID.ToString());
                    model.PfaOrgID = data.PfaOrgID;
                    model.OrgList = GetOrgList(data.PfaOrgID.ToString(), data.PfaCycleID);
                    model.OrgTotal = data.OrgTotal;

                    var PfaOrg = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrgID == data.PfaOrgID).Select(x => x.PfaDept.PfaDeptName).ToList();
                    string str = "";
                    foreach (var dept in PfaOrg)
                    {
                        if (!string.IsNullOrEmpty(str)) str += "、";
                        str += dept;
                    }
                    model.DeptName = str;

                    var Detail = Services.GetService<PfaCycleRationDetailService>().GetPfaCycleRationDetail(id);

                    model.StaffingTotal = Detail.Select(x => x.Staffing).Sum();
                    model.ScoreTotal = Detail.Select(x => x.TotalScore).Sum();

                    model.Data = new PfaCycleRationDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaCycleRationDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaCycleRationID = Detail[i].PfaCycleRationID;
                        model.Data[i].PfaPerformanceID = Detail[i].PfaPerformanceID;
                        model.Data[i].Code = Detail[i].Code;
                        model.Data[i].Name = Detail[i].Name;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].Performance = Detail[i].Performance;
                        model.Data[i].band = Detail[i].band;
                        model.Data[i].Rates = Detail[i].Rates;
                        model.Data[i].Multiplier = Detail[i].Multiplier;
                        model.Data[i].Total = data.OrgTotal.Value;
                        model.Data[i].BudgetTotal = Detail[i].BudgetTotal;
                        model.Data[i].Staffing = Detail[i].Staffing;
                        model.Data[i].TotalScore = Detail[i].TotalScore;
                    }
                    return PartialView("_DetailPfaCycleRation", model);
                }
            }
            else
            {
                return PartialView("_DetailPfaCycleRation");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(Guid? id)
        {
            if (id.HasValue)
            {
                PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();

                PfaCycleRation data = Services.GetService<PfaCycleRationService>().GetPfaCycleRation(id);
                if (data == null)
                {
                    return PartialView("_DeletePfaCycleRation");
                }
                else
                {
                    model.ID = id.Value;
                    model.PfaCycleID = data.PfaCycleID;
                    model.CycleList = GetCycleList(data.PfaCycleID.ToString());
                    model.PfaOrgID = data.PfaOrgID;
                    model.OrgList = GetOrgList(data.PfaOrgID.ToString(), data.PfaCycleID);
                    model.OrgTotal = data.OrgTotal;

                    var PfaOrg = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrgID == data.PfaOrgID).Select(x => x.PfaDept.PfaDeptName).ToList();
                    string str = "";
                    foreach (var dept in PfaOrg)
                    {
                        if (!string.IsNullOrEmpty(str)) str += "、";
                        str += dept;
                    }
                    model.DeptName = str;

                    var Detail = Services.GetService<PfaCycleRationDetailService>().GetPfaCycleRationDetail(id);

                    model.StaffingTotal = Detail.Select(x => x.Staffing).Sum();
                    model.ScoreTotal = Detail.Select(x => x.TotalScore).Sum();

                    model.Data = new PfaCycleRationDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new PfaCycleRationDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaCycleRationID = Detail[i].PfaCycleRationID;
                        model.Data[i].PfaPerformanceID = Detail[i].PfaPerformanceID;
                        model.Data[i].Code = Detail[i].Code;
                        model.Data[i].Name = Detail[i].Name;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].Performance = Detail[i].Performance;
                        model.Data[i].band = Detail[i].band;
                        model.Data[i].Rates = Detail[i].Rates;
                        model.Data[i].Multiplier = Detail[i].Multiplier;
                        model.Data[i].Total = data.OrgTotal.Value;
                        model.Data[i].BudgetTotal = Detail[i].BudgetTotal;
                        model.Data[i].Staffing = Detail[i].Staffing;
                        model.Data[i].TotalScore = Detail[i].TotalScore;
                    }
                    return PartialView("_DeletePfaCycleRation", model);
                }
            }
            else
            {
                return PartialView("_DeletePfaCycleRation");
            }
        }

        [HttpPost]
        public ActionResult DeletePfaCycleRation(Guid? id)
        {
            Result result = Services.GetService<PfaCycleRationService>().DeletePfaCycleRation(id.Value);
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 共用
        /// <summary>
        /// 績效考核批號
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetCycleList(string selecteddata, string Action = null)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaCycle> data = new List<PfaCycle>();
            if (Action == "c")
            {
                data = Services.GetService<PfaCycleService>().GetAll().Where(x => x.Status == "m").OrderBy(x => x.PfaYear).ThenBy(x => x.PfaFormNo).ToList();
            }
            else
            {
                data = Services.GetService<PfaCycleService>().GetAll().OrderBy(x => x.PfaYear).ThenBy(x => x.PfaFormNo).ToList();
            }

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }

            return listItem;
        }

        public ActionResult QueryOrgList(Guid pfaCycleID)
        {
            var result = new List<SelectListItem>();

            try
            {
                result = GetOrgList("", pfaCycleID, true);
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("取得績效考核組織失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "取得績效考核組織失敗", data = string.Empty });
            }
            return Json(new { success = true, message = "", data = result });
        }

        /// <summary>
        /// 績效考核組織
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <param name="PfaCycleID"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        private List<SelectListItem> GetOrgList(string selecteddata, Guid? PfaCycleID = null, bool exclude = false)
        {
            var data = Services.GetService<PfaOrgService>().GetAll().OrderBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();


            if (PfaCycleID.HasValue)
            {
                var pfaOrgIDs = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == PfaCycleID && x.PfaOrgID.HasValue && x.IsRatio).Select(x => x.PfaOrgID.Value).Distinct().ToList();
                data = data.Where(x=> pfaOrgIDs.Contains(x.ID)).OrderBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();
            }

            if (exclude)
            {
                var pfaOrgIDs = Services.GetService<PfaCycleRationService>().GetAll().Where(x => x.PfaCycleID == PfaCycleID).Select(x => x.PfaOrgID).Distinct().ToList();
                data = data.Where(x => !pfaOrgIDs.Contains(x.ID)).OrderBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();
            }

            List<SelectListItem> listItem = new List<SelectListItem>();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
                SelectedDataID = Guid.Parse(selecteddata);

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.PfaOrgName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }

        /// <summary>
        /// 試算
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult Calculation(PfaCycleRationCreateViewModel obj)
        {
            PfaCycleRationCreateViewModel model = new PfaCycleRationCreateViewModel();
            try
            {
                int StaffingTotal = 0;
                decimal ScoreTotal = 0;

                if (obj.Action == "Edit")
                {
                    var PfaCycleRationDetail = Services.GetService<PfaCycleRationDetailService>().GetPfaCycleRationDetail(obj.ID);

                    foreach (var item in obj.Data)
                    {
                        var Detail = PfaCycleRationDetail.Where(x => x.ID == item.ID).FirstOrDefault();
                        if (Detail != null)
                        {
                            item.ID = Detail.ID;
                            item.PfaCycleRationID = Detail.PfaCycleRationID;
                            item.PfaPerformanceID = Detail.PfaPerformanceID;
                            item.Code = Detail.Code;
                            item.Name = Detail.Name;
                            item.Ordering = Detail.Ordering;
                            item.Performance = Detail.Performance;
                            item.Rates = Detail.Rates;
                            item.band = Detail.band;
                            item.Multiplier = Detail.Multiplier;
                            item.Total = obj.OrgTotal.Value;

                            double BudgetTotal = Convert.ToDouble(obj.OrgTotal * Detail.Rates * Detail.Multiplier / 100);
                            BudgetTotal = BudgetTotal * 100;
                            BudgetTotal = Math.Floor(BudgetTotal);
                            BudgetTotal = BudgetTotal / 100;

                            item.BudgetTotal = Convert.ToDecimal(BudgetTotal.ToString("#0.0"));

                            item.TotalScore = item.Staffing * Detail.Multiplier;
                        }

                    }
                }
                else
                {
                    var PerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).ToList();


                    foreach (var item in obj.Data)
                    {
                        var Performance = PerformanceList.Where(x => x.ID == item.PfaPerformanceID).FirstOrDefault();
                        if (Performance != null)
                        {
                            item.PfaPerformanceID = Performance.ID;
                            item.Code = Performance.Code;
                            item.Name = Performance.Name;
                            item.Ordering = Performance.Ordering;
                            item.Performance = Performance.Performance;
                            item.Rates = Performance.Rates;
                            item.band = Performance.band;
                            item.Multiplier = Performance.Multiplier;
                            item.Total = obj.OrgTotal.Value;

                            double BudgetTotal = Convert.ToDouble(obj.OrgTotal * Performance.Rates * Performance.Multiplier / 100);
                            BudgetTotal = BudgetTotal * 100;
                            BudgetTotal = Math.Floor(BudgetTotal);
                            BudgetTotal = BudgetTotal / 100;

                            item.BudgetTotal = Convert.ToDecimal(BudgetTotal.ToString("#0.0"));

                            item.TotalScore = item.Staffing * Performance.Multiplier;
                        }
                    }
                }

                StaffingTotal = obj.Data.Select(x => x.Staffing.Value).Sum();
                ScoreTotal = obj.Data.Select(x => x.TotalScore.Value).Sum();

                StringBuilder dataResult = new StringBuilder();
                foreach (var item in obj.Data)
                {
                    dataResult.Append(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr'>"));
                    dataResult.Append(string.Format("<input id='CycleRationDetailID' name='CycleRationDetailID' type='hidden' value='{0}'>", item.ID));
                    dataResult.Append(string.Format("<input id='PfaCycleRationID' name='PfaCycleRationID' type='hidden' value='{0}'>", item.PfaCycleRationID));
                    dataResult.Append(string.Format("<input id='PfaPerformanceID' name='PfaPerformanceID' type='hidden' value='{0}'>", item.PfaPerformanceID));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Performance));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Rates));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.band));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Multiplier));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.Total));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}</td>", item.BudgetTotal));
                    dataResult.Append(string.Format("<td style='white-space:normal;text-align:center;'><input class='form-control' id='Staffing' name='Staffing' type='text' value='{0}'></td>", item.Staffing));
                    dataResult.Append(string.Format("<td id='CalStaffing' name='CalStaffing' style='white-space:normal;text-align:center;'>{0}</td>", item.TotalScore));
                    dataResult.Append("</tr>");
                }

                StringBuilder dataEnd = new StringBuilder();
                dataEnd.Append(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr'>"));
                dataEnd.Append(string.Format("<td style='white-space:normal;text-align:right;' colspan='6'>{0}</td>", "合計"));
                dataEnd.Append(string.Format("<td style='white-space:normal;text-align:center;'>{0}　<button type='button' id='btnCal' class='btn btn-primary' style:'float:right'>試算</button></td>", StaffingTotal));
                dataEnd.Append(string.Format("<td id='ScoreTotal' name='ScoreTotal' style='white-space:normal;text-align:center;'>{0}</td>", ScoreTotal));
                dataEnd.Append("</tr>");

                return Json(new { success = true, message = "", detail = model, data = dataResult.ToString(), dataEnd = dataEnd.ToString() });
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("取得組織應配比人數失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "取得組織應配比人數失敗", detail = model, data = string.Empty });
            }
        }
        #endregion

        #region JobTitle
        public ActionResult JobTitle()
        {
            PfaRatioJobTitleViewModel model = new PfaRatioJobTitleViewModel();
            List<Guid> JobTitle = Services.GetService<PfaRatioJobTitleService>().GetAll().Select(x => x.JobTitleID).ToList();
            model.JobTitleItems = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "JobTitle").OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).Select(x => new JobTitleItem()
            {
                ID = x.ID,
                Code = x.OptionCode,
                Name = x.OptionName,
                Chk = JobTitle.Contains(x.ID)
            }).ToList();
            return PartialView("_PfaRatioJobTitle", model);
        }

        [HttpPost]
        public ActionResult PfaRatioJobTitle(string SelJboTitleID)
        {
            Result result = null;
            result = Services.GetService<PfaRatioJobTitleService>().SaveJobTitle(SelJboTitleID, CurrentUser.EmployeeID);
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion
    }
}