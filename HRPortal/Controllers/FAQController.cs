using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using HRPortal.DBEntities;

namespace HRPortal.Controllers
{
    public class FAQController : BaseController
    {
        //
        // GET: /FAQ/
        public ActionResult Index(int page = 1, string keyword = "", string FAQTypeData = "")
        {
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<FAQService>().GetFAQList();

            ViewBag.selectFAQType = FAQTypeData;
            ViewBag.TitleKeyword = keyword;
            ViewData["TypeList"] = GetFAQType(FAQTypeData);
            if (!string.IsNullOrEmpty(keyword))
            {
                ds = ds.Where(x => x.Title.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(FAQTypeData))
            {
                Guid typeId = Guid.Parse(FAQTypeData);
                ds = ds.Where(x => x.Type == typeId);
            }
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
         string SearchTitle, string txtkeyword, int page = 1, string FAQTypeData = "")
        {

            if (!string.IsNullOrWhiteSpace(FAQTypeData) || (SearchTitle == "1" && txtkeyword != null && txtkeyword != "篩選標題"))
            {
                page = 1;

                return RedirectToAction("Index", new
                {
                    page = page,
                    keyword = txtkeyword,
                    FAQTypeData = FAQTypeData
                });
            }
            //重整
            var ds = Services.GetService<FAQService>().GetFAQList();
            ViewData["TypeList"] = GetFAQType(FAQTypeData);
            ViewBag.selectFAQType = FAQTypeData;
            ViewBag.TitleKeyword = "";
            int currentPage = page < 1 ? 1 : page;
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        /// <summary>
        /// FAQ類別選單
        /// </summary>
        /// <param name="selecteddata">選取中的資料</param>
        /// <returns></returns>
        private List<SelectListItem> GetFAQType(string selecteddata)
        {
            List<FAQType> typeData = Services.GetService<FAQTypeService>().GetFAQTypeList().ToList();
            List<SelectListItem> listItem = new List<SelectListItem>();
            //轉為Guid 判斷ID
            Guid SelectedFAQType = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedFAQType = Guid.Parse(selecteddata);
            }
            listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in typeData)
            {
                listItem.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString(), Selected = (SelectedFAQType == item.Id ? true : false) });
            }
            return listItem;
        }

        public ActionResult Detail(Guid id)
        {
            FAQ model = new FAQ();
            model = Services.GetService<FAQService>().GetFAQByID(id);
            return PartialView("_Detail", model);
        }

        public ActionResult AnswerFAQ()
        {
            SetBaseUserInfo();
            return PartialView("_AskFAQ");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CreateAnswerFAQ(AnswerFAQ model)
        {
            SetBaseUserInfo();
            if (!ModelState.IsValid)
            {
                return PartialView("_AskFAQ", model);
            }
            else
            {
                model.ID = Guid.NewGuid();
                model.EmployeeID = CurrentUser.EmployeeID;
                model.CompanyID = CurrentUser.CompanyID;
                model.DepartmentID = CurrentUser.DepartmentID;
                model.Createdby = CurrentUser.EmployeeID;

                int IsSuccess = Services.GetService<AnswerFAQService>().Create(model, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "成功";
                    // ModelState.Clear();
                    WriteLog("Success:" + model.ID);
                    string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
                    SendMail(new string[] { _fromMail },
                   new string[] { CurrentUser.Employee.Email }, new string[] { }, "詢問問題:" + model.Title, string.Format("詢問內容:{0}<br><br>提問者:【{1}】{2}<br>", model.ContentText.Replace("\r\n", "<br/>"), CurrentUser.Employee.Department.DepartmentName,
                   CurrentUser.Employee.EmployeeName), true);
                    return Json(new { success = true });
                }
            }
            return PartialView("_AskFAQ", model);
        }

    }
}