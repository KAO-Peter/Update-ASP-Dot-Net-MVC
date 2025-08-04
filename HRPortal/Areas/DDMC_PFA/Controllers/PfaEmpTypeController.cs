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
    public class PfaEmpTypeController : BaseController
    {
        #region Index
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtEmpTypeCode = "", string txtEmpTypeName = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtEmpTypeCode, txtEmpTypeName);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaEmpTypeService>().GetPfaEmpTypeData(txtEmpTypeCode, txtEmpTypeName);

            var viewModel = ds.Select(x => new PfaEmpTypeViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                PfaEmpTypeCode = x.PfaEmpTypeCode,
                PfaEmpTypeName = x.PfaEmpTypeName,
                JobTitleIDList = x.PfaEmpTypeTargets.Select(y => y.JobTitleID).ToList()
            }).ToPagedList(currentPage, currentPageSize);

            foreach (var item in viewModel)
            {
                string str = "";

                var datas = Services.GetService<PfaOptionService>().GetAll().Where(x => item.JobTitleIDList.Contains(x.ID)).Select(x => x.OptionName).ToList();

                foreach (var Option in datas)
                {
                    if (!string.IsNullOrEmpty(str)) str += "、";
                    str += Option;
                }
                item.JobTitleName = str;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtCompanyID, string txtEmpTypeCode, string txtEmpTypeName, string btnQuery, string btnClear)
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
                    txtEmpTypeCode,
                    txtEmpTypeName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtEmpTypeCode, txtEmpTypeName);
            return View();
        }

        private void GetDefaultData(string txtCompanyID = "", string txtEmpTypeCode = "", string txtEmpTypeName = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }
            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtOrgCode = txtEmpTypeCode;
            ViewBag.txtOrgName = txtEmpTypeName;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            CreatePfaEmpType model = new CreatePfaEmpType();
            model.ID = Guid.Empty;
            model.CompanyID = CurrentUser.CompanyID;
            return PartialView("_CreatePfaEmpType", model);
        }

        [HttpPost]
        public ActionResult Create(CreatePfaEmpType model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var isExist = Services.GetService<PfaEmpTypeService>().IsExist(model.PfaEmpTypeCode);
                if (isExist)
                {
                    WriteLog(string.Format("身分類別資料已存在,Code:{0}", model.PfaEmpTypeCode));
                    return Json(new { success = false, message = "身分類別資料已存在" });
                }

                PfaEmpType data = new PfaEmpType();
                data.ID = Guid.NewGuid();
                data.PfaEmpTypeCode = model.PfaEmpTypeCode;
                data.PfaEmpTypeName = model.PfaEmpTypeName;
                data.IsUsed = true;
                data.CompanyID = model.CompanyID;
                data.CreatedBy = CurrentUser.EmployeeID;
                data.CreatedTime = DateTime.Now;

                int IsSuccess = Services.GetService<PfaEmpTypeService>().Create(data);
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
            PfaEmpType data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaEmpTypeService>().GetPfaEmpType(id);
            }

            CreatePfaEmpType model = new CreatePfaEmpType();
            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaEmpTypeCode = data.PfaEmpTypeCode;
                model.PfaEmpTypeName = data.PfaEmpTypeName;
                model.Ordering = data.Ordering;
            }
            return PartialView("_EditPfaEmpType", model);
        }

        [HttpPost]
        public ActionResult Edit(CreatePfaEmpType model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var data = Services.GetService<PfaEmpTypeService>().GetPfaEmpType(model.ID);
                if (data == null)
                {
                    WriteLog(string.Format("查無此身分類別組織,ID:{0}", model.ID));
                    return Json(new { success = false, message = "查無此身分類別組織" });
                }
                else
                {
                    data.PfaEmpTypeCode = model.PfaEmpTypeCode;
                    data.PfaEmpTypeName = model.PfaEmpTypeName;
                    data.IsUsed = true;
                    data.Ordering = model.Ordering;
                    data.CompanyID = model.CompanyID;
                    data.ModifiedBy = CurrentUser.EmployeeID;
                    data.ModifiedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaEmpTypeService>().Update(data);
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

        #region Detail
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(Guid? id)
        {
            PfaEmpType data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaEmpTypeService>().GetPfaEmpType(id);
            }

            CreatePfaEmpType model = new CreatePfaEmpType();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaEmpTypeCode = data.PfaEmpTypeCode;
                model.PfaEmpTypeName = data.PfaEmpTypeName;
                model.Ordering = data.Ordering;
            }

            return PartialView("_DetailPfaEmpType", model);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Del
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Del(Guid? id)
        {
            PfaEmpType data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaEmpTypeService>().GetPfaEmpType(id);
            }

            CreatePfaEmpType model = new CreatePfaEmpType();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaEmpTypeCode = data.PfaEmpTypeCode;
                model.PfaEmpTypeName = data.PfaEmpTypeName;
                model.Ordering = data.Ordering;
            }
            return PartialView("_DeletePfaEmpType", model);
        }

        /// <summary>
        /// DelPfaDept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(Guid? id)
        {
            Result result = null;

            PfaEmpType data = Services.GetService<PfaEmpTypeService>().GetPfaEmpType(id);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此身分類別組織";
                result.log = "查無此身分類別組織";
            }
            else
            {
                result = Services.GetService<PfaEmpTypeService>().DeletePfaEmpType(id.Value);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region SelPfaEmpTypeTargets
        public ActionResult SelPfaEmpTypeTargets(Guid? id)
        {
            SelJobTitleFunction model = Services.GetService<PfaEmpTypeService>().GetAll().Where(x => x.ID == id).Select(x => new SelJobTitleFunction()
            {
                PfaEmpTypeID = x.ID,
                PfaEmpTypeCode = x.PfaEmpTypeCode,
                PfaEmpTypeName = x.PfaEmpTypeName
            }).FirstOrDefault();

            if (model == null)
            {
                model = new SelJobTitleFunction();
                model.PfaEmpTypeID = Guid.Empty;
            }
            else
            {
                var jobTitleIDList = Services.GetService<PfaEmpTypeTargetsService>().GetAll().Select(x => x.JobTitleID).ToList();
                List<Guid> OrgDeptID = Services.GetService<PfaEmpTypeTargetsService>().GetJobTitleID(model.PfaEmpTypeID);
                model.JobTitleItems = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "JobTitle").OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).Select(x => new JobTitleItem()
                {
                    ID = x.ID,
                    Code = x.OptionCode,
                    Name = x.OptionName,
                    Chk = OrgDeptID.Contains(x.ID),
                    Setucompleted = jobTitleIDList.Contains(x.ID),
                }).ToList();
            }
            return PartialView("_SelPfaEmpTypeTargets", model);
        }

        [HttpPost]
        public ActionResult SelPfaEmpTypeTargets(Guid PfaEmpTypeID, string SelJboTitleID)
        {
            Result result = null;

            var data = Services.GetService<PfaEmpTypeService>().GetAll().Where(x => x.ID == PfaEmpTypeID);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此身分類別組織";
                result.log = "查無此身分類別組織";
            }
            else
            {
                result = Services.GetService<PfaEmpTypeTargetsService>().SaveJobTitle(PfaEmpTypeID, SelJboTitleID, CurrentUser.EmployeeID);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion
    }
}