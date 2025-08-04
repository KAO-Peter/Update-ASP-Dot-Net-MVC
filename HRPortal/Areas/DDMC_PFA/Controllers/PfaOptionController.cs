using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Services.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaOptionController : BaseController
    {
        #region Index
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewData["PfaOptionGroupList"] = GetPfaOptionList(false);
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isAdmin"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public List<SelectListItem> GetPfaOptionList(bool isAdmin, string selectedValue = null)
        {
            List<SelectListItem> _listItem = new List<SelectListItem>();
            IList<PfaOptionGroup> _options = Services.GetService<PfaOptionGroupService>().GetPfaOptionGroup(isAdmin);
            foreach (PfaOptionGroup _option in _options)
            {
                SelectListItem _item = new SelectListItem();
                _item.Value = _option.ID.ToString();
                if (!string.IsNullOrEmpty(selectedValue) && selectedValue == _option.ID.ToString())
                {
                    _item.Selected = true;
                }
                _item.Text = _option.OptionGroupName;
                _listItem.Add(_item);
            }

            return _listItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PfaCodeGroup"></param>
        /// <returns></returns>
        public ActionResult LoadPfaOptionList(Guid? PfaCodeGroup)
        {
            List<PfaOption> model = Services.GetService<PfaOptionService>().GetPfaOption(PfaCodeGroup, false).OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).ToList();
            return PartialView("_PfaOptionList", model);
        }
        #endregion

        #region Add
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPfaOption()
        {
            return PartialView("_AddPfaOption");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddPfaOption(PfaOption model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var isExist = Services.GetService<PfaOptionService>().IsExist(model.PfaOptionGroupID, model.OptionCode);

                if (isExist)
                {
                    WriteLog(string.Format("考核選項資料已存在,GroupID:{0},Code:{1}", model.PfaOptionGroupID, model.OptionCode));
                    return Json(new { success = false, message = "考核選項資料已存在" });
                }
                else
                {
                    model.ID = Guid.NewGuid();
                    model.CreatedBy = CurrentUser.EmployeeID;
                    model.CreatedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaOptionService>().Create(model);
                    if (IsSuccess == 1)
                    {
                        WriteLog(string.Format("新增成功,ID:{0}", model.ID));
                        return Json(new { success = true, message = "新增成功" });
                    }
                }
            }

            return Json(new { success = true, message = "新增成功" });
        }
        #endregion

        #region Edit
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditPfaOption(Guid? id)
        {
            if (id.HasValue)
            {
                PfaOption model = Services.GetService<PfaOptionService>().GetPfaOption(id);
                return PartialView("_EditPfaOption", model);
            }
            else
                return PartialView("_EditPfaOption");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditPfaOption(PfaOption model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                PfaOption data = Services.GetService<PfaOptionService>().GetPfaOption(model.ID);

                if (data == null)
                {
                    WriteLog(string.Format("查無此考核選項資料,ID:{0}", model.ID));
                    return Json(new { success = false, message = "查無此考核選項資料" });
                }
                else
                {
                    data.OptionName = model.OptionName;
                    data.Ordering = model.Ordering;
                    data.ModifiedBy = CurrentUser.EmployeeID;
                    data.ModifiedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaOptionService>().Update(data);
                    if (IsSuccess == 1)
                    {
                        WriteLog(string.Format("編輯成功,ID:{0}", model.ID));
                        return Json(new { success = true, message = "編輯成功" });
                    }
                }
            }

            return Json(new { success = true, message = "編輯成功" });
        }
        #endregion

        #region Delete
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelPfaOption(Guid? id)
        {
            if (id.HasValue)
            {
                PfaOption model = Services.GetService<PfaOptionService>().GetPfaOption(id);
                return PartialView("_DelPfaOption", model);
            }
            else
                return PartialView("_DelPfaOption");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeletePfaOption(Guid? id)
        {
            PfaOption data = Services.GetService<PfaOptionService>().GetPfaOption(id);

            if (data == null)
            {
                WriteLog(string.Format("查無此考核選項資料,ID:{0}", id));
                return Json(new { success = false, message = "查無此考核選項資料" });
            }
            else
            {
                try
                {
                    Services.GetService<PfaOptionService>().Delete(data);
                    WriteLog(string.Format("刪除成功,ID:{0}", data.ID));
                    return Json(new { success = true, message = "刪除成功" });
                }
                catch
                {
                    WriteLog(string.Format("刪除失敗,ID:{0}", data.ID));
                    return Json(new { success = false, message = "刪除失敗" });
                }
            }
        }
        #endregion
    }
}