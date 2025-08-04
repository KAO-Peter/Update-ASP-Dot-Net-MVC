using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaAbilityController : BaseController
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
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtEmpTypeID = "", string txtAbilityCode = "", string txtAbilityName = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtEmpTypeID, txtAbilityCode, txtAbilityName);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaAbilityService>().GetPfaAbilityData(string.IsNullOrWhiteSpace(txtCompanyID) ? Guid.Empty : Guid.Parse(txtCompanyID), string.IsNullOrWhiteSpace(txtEmpTypeID) ? Guid.Empty : Guid.Parse(txtEmpTypeID), txtAbilityCode, txtAbilityName);

            var viewModel = ds.Select(x => new PfaAbilityViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                PfaEmpTypeID = x.PfaEmpTypeID,
                PfaEmpTypeName = x.PfaEmpType.PfaEmpTypeName,
                PfaAbilityCode = x.PfaAbilityCode,
                PfaAbilityName = x.PfaAbilityName,
                Description = x.Description,
                TotalScore = x.TotalScore,
                Ordering = x.Ordering,
            }).ToPagedList(currentPage, currentPageSize);

            foreach (var item in viewModel)
            {
                item.AbilityDetail = Services.GetService<PfaAbilityDetailService>().GetAll().Where(y => y.PfaAbilityID == item.ID).OrderBy(y => y.Ordering).Select(y => new PfaAbilityDetailViewModel() { Ordering = y.Ordering, PfaAbilityKey = y.PfaAbilityKey }).ToList();
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
        public ActionResult Index(string txtCompanyID, string txtEmpTypeID, string txtAbilityCode, string txtAbilityName, string btnQuery, string btnClear)
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
                    txtEmpTypeID,
                    txtAbilityCode,
                    txtAbilityName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtEmpTypeID, txtAbilityCode, txtAbilityName);

            return View();
        }

        /// <summary>
        /// 取得預設值
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        private void GetDefaultData(string txtCompanyID = "", string txtEmpTypeID = "", string txtAbilityCode = "", string txtAbilityName = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }

            ViewData["EmpTypeList"] = GetEmpTypeList(txtEmpTypeID);

            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtAbilityCode = txtAbilityCode;
            ViewBag.txtAbilityName = txtAbilityName;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            PfaAbilityCreateViewModel model = new PfaAbilityCreateViewModel();
            model.ID = Guid.Empty;
            model.EmpTypeList = GetEmpTypeList(string.Empty);

            return PartialView("_CreatePfaAbility", model);
        }

        [HttpPost]
        public ActionResult Create(PfaAbilityCreateViewModel obj)
        {
            if (obj.PfaEmpTypeID == null || obj.PfaEmpTypeID == Guid.Empty)
            {
                WriteLog("請選擇身份類別");
                return Json(new { success = false, message = "請選擇身份類別" });
            }
            else if (obj.Data.Length == 0)
            {
                WriteLog("請輸入關鍵行為指標");
                return Json(new { success = false, message = "請輸入關鍵行為指標" });
            }
            else
            {
                var isExist = Services.GetService<PfaAbilityService>().GetAll().Where(x => x.PfaEmpTypeID == obj.PfaEmpTypeID && x.PfaAbilityCode == obj.PfaAbilityCode).Any();
                if (isExist)
                {
                    WriteLog(string.Format("勝任能力代碼已存在,PfaAbilityCode:{0}", obj.ID));
                    return Json(new { success = false, message = "勝任能力代碼已存在" });
                }

                PfaAbility PfaAbility = new PfaAbility();
                PfaAbility.ID = Guid.NewGuid();
                PfaAbility.PfaEmpTypeID = obj.PfaEmpTypeID;
                PfaAbility.PfaAbilityCode = obj.PfaAbilityCode;
                PfaAbility.PfaAbilityName = obj.PfaAbilityName;
                PfaAbility.Description = obj.Description;
                PfaAbility.TotalScore = obj.TotalScore;
                PfaAbility.IsUsed = true;
                PfaAbility.Ordering = obj.Ordering;
                PfaAbility.CompanyID = CurrentUser.CompanyID;
                PfaAbility.CreatedBy = CurrentUser.EmployeeID;
                PfaAbility.CreatedTime = DateTime.Now;

                List<PfaAbilityDetail> PfaAbilityDetail = new List<PfaAbilityDetail>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaAbilityDetail flow = new PfaAbilityDetail();
                    flow.ID = Guid.NewGuid();
                    flow.PfaAbilityID = PfaAbility.ID;
                    flow.PfaAbilityKey = item.PfaAbilityKey;
                    flow.UpperLimit = item.UpperLimit;
                    flow.LowerLimit = item.LowerLimit;
                    flow.Ordering = item.Ordering;
                    flow.CreatedBy = CurrentUser.EmployeeID;
                    flow.CreatedTime = DateTime.Now;
                    PfaAbilityDetail.Add(flow);
                }

                if (PfaAbility != null && PfaAbilityDetail.Count > 0)
                {
                    Result result = Services.GetService<PfaAbilityService>().CreatePfaAbility(PfaAbility, PfaAbilityDetail);
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
                PfaAbilityCreateViewModel model = new PfaAbilityCreateViewModel();

                PfaAbility data = Services.GetService<PfaAbilityService>().GetPfaAbility(id);
                if (data == null)
                {
                    return PartialView("_EditPfaAbility");
                }
                else
                {
                    model.ID = id.Value;
                    model.EmpTypeList = GetEmpTypeList(data.PfaEmpTypeID.ToString());
                    model.Ordering = data.Ordering;
                    model.PfaAbilityCode = data.PfaAbilityCode;
                    model.PfaAbilityName = data.PfaAbilityName;
                    model.TotalScore = data.TotalScore;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaAbilityDetailService>().GetAbilityDetail(id);

                    model.Data = new AbilityDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new AbilityDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaAbilityID = Detail[i].PfaAbilityID;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].PfaAbilityKey = Detail[i].PfaAbilityKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                    }
                    return PartialView("_EditPfaAbility", model);
                }
            }
            else
            {
                return PartialView("_EditPfaAbility");
            }
        }

        [HttpPost]
        public ActionResult Edit(PfaAbilityCreateViewModel obj)
        {
            if (obj.ID == null || obj.ID == Guid.Empty)
            {
                WriteLog("請選擇身份類別");
                return Json(new { success = false, message = "請選擇身份類別" });
            }
            else if (obj.Data.Length == 0)
            {
                WriteLog("請輸入關鍵行為指標");
                return Json(new { success = false, message = "請輸入關鍵行為指標" });
            }
            else
            {
                PfaAbility PfaAbility = new PfaAbility();
                PfaAbility.ID = obj.ID;
                PfaAbility.PfaEmpTypeID = obj.PfaEmpTypeID;
                PfaAbility.PfaAbilityCode = obj.PfaAbilityCode;
                PfaAbility.PfaAbilityName = obj.PfaAbilityName;
                PfaAbility.Description = obj.Description;
                PfaAbility.TotalScore = obj.TotalScore;
                PfaAbility.IsUsed = true;
                PfaAbility.Ordering = obj.Ordering;
                PfaAbility.CompanyID = CurrentUser.CompanyID;
                PfaAbility.ModifiedBy = CurrentUser.EmployeeID;
                PfaAbility.ModifiedTime = DateTime.Now;

                List<PfaAbilityDetail> PfaAbilityDetail = new List<PfaAbilityDetail>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaAbilityDetail flow = new PfaAbilityDetail();
                    flow.ID = Guid.NewGuid();
                    flow.PfaAbilityID = obj.ID;
                    flow.PfaAbilityKey = item.PfaAbilityKey;
                    flow.UpperLimit = item.UpperLimit;
                    flow.LowerLimit = item.LowerLimit;
                    flow.Ordering = item.Ordering;
                    flow.CreatedBy = CurrentUser.EmployeeID;
                    flow.CreatedTime = DateTime.Now;
                    PfaAbilityDetail.Add(flow);
                }

                if (PfaAbility != null && PfaAbilityDetail.Count > 0)
                {
                    Result result = Services.GetService<PfaAbilityService>().EditPfaAbility(PfaAbility, PfaAbilityDetail);
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
                PfaAbilityCreateViewModel model = new PfaAbilityCreateViewModel();

                PfaAbility data = Services.GetService<PfaAbilityService>().GetPfaAbility(id);
                if (data == null)
                {
                    return PartialView("_DetailPfaAbility");
                }
                else
                {
                    model.ID = id.Value;
                    model.EmpTypeList = GetEmpTypeList(data.PfaEmpTypeID.ToString());
                    model.Ordering = data.Ordering;
                    model.PfaAbilityCode = data.PfaAbilityCode;
                    model.PfaAbilityName = data.PfaAbilityName;
                    model.TotalScore = data.TotalScore;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaAbilityDetailService>().GetAbilityDetail(id);

                    model.Data = new AbilityDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new AbilityDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaAbilityID = Detail[i].PfaAbilityID;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].PfaAbilityKey = Detail[i].PfaAbilityKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                    }
                    return PartialView("_DetailPfaAbility", model);
                }
            }
            else
            {
                return PartialView("_DetailPfaAbility");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(Guid? id)
        {
            if (id.HasValue)
            {
                PfaAbilityCreateViewModel model = new PfaAbilityCreateViewModel();

                PfaAbility data = Services.GetService<PfaAbilityService>().GetPfaAbility(id);
                if (data == null)
                {
                    return PartialView("_DeletePfaAbility");
                }
                else
                {
                    model.ID = id.Value;
                    model.EmpTypeList = GetEmpTypeList(data.PfaEmpTypeID.ToString());
                    model.Ordering = data.Ordering;
                    model.PfaAbilityCode = data.PfaAbilityCode;
                    model.PfaAbilityName = data.PfaAbilityName;
                    model.TotalScore = data.TotalScore;
                    model.Description = data.Description;

                    var Detail = Services.GetService<PfaAbilityDetailService>().GetAbilityDetail(id);

                    model.Data = new AbilityDetailCreateViewModel[Detail.Count];
                    for (int i = 0; i < Detail.Count; i++)
                    {
                        model.Data[i] = new AbilityDetailCreateViewModel();
                        model.Data[i].ID = Detail[i].ID;
                        model.Data[i].PfaAbilityID = Detail[i].PfaAbilityID;
                        model.Data[i].Ordering = Detail[i].Ordering;
                        model.Data[i].PfaAbilityKey = Detail[i].PfaAbilityKey;
                        model.Data[i].UpperLimit = Detail[i].UpperLimit;
                        model.Data[i].LowerLimit = Detail[i].LowerLimit;
                    }
                    return PartialView("_DeletePfaAbility", model);
                }
            }
            else
            {
                return PartialView("_DeletePfaAbility");
            }
        }

        [HttpPost]
        public ActionResult DeletePfaAbility(Guid? id)
        {
            Result result = Services.GetService<PfaAbilityService>().DeletePfaAbility(id.Value);
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 共用
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmpTypeList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaEmpType> data = Services.GetService<PfaEmpTypeService>().GetAll().OrderBy(x => x.Ordering).ThenBy(x => x.PfaEmpTypeCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.PfaEmpTypeName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }

            return listItem;
        }
        #endregion
    }
}