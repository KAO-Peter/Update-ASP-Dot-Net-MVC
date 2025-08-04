using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using HRPortal.Mvc.Models;
using HRPortal.DBEntities;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M06.Controllers
{
    public class VacanciesManagerController : BaseController
    {
        // GET: /M06/VacanciesManager/
        public ActionResult Index(int page = 1, string keyword = "")
        {
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<VacanciesService>().GetFAQList();
           
            if (!string.IsNullOrEmpty(keyword))
            {
                ds = ds.Where(x => x.Title.Contains(keyword));
            }
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
         string SearchTitle, string txtkeyword, int page = 1)
        {
            var ds = Services.GetService<VacanciesService>().GetFAQList();

            if (SearchTitle == "1" && txtkeyword != null && txtkeyword != "篩選標題")
            {
                page = 1;

                return RedirectToAction("Index", new
                {
                    page = page,
                    keyword = txtkeyword
                });
            }
            //重整
            int currentPage = page < 1 ? 1 : page;
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }


        public ActionResult Create()
        {
            SetBaseUserInfo();
            VacanciesViewModel viewModel = new VacanciesViewModel();
            List<VacanciesType> typeModel = Services.GetService<VacanciesTypeService>().GetFAQTypeList().ToList();
            Vacancies FAQModel = new Vacancies();
            FAQModel.Createdby = CurrentUser.Employee.ID;
            viewModel.FAQTypeLists = typeModel;
            viewModel.Data = FAQModel;


            return PartialView("_CreateFAQ", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(VacanciesViewModel model)
        {
            SetBaseUserInfo();
            List<VacanciesType> typeModel = Services.GetService<VacanciesTypeService>().GetFAQTypeList().ToList();
            model.FAQTypeLists = typeModel;
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateFAQ", model);
            }
            else if (model.Data.Type == Guid.Empty && string.IsNullOrWhiteSpace(model.OtherType))
            {
                TempData["message"] = "請輸入其他";
            }
            else if (model.Data.Type == Guid.Empty && string.IsNullOrWhiteSpace(model.OtherType) && Services.GetService<FAQTypeService>().CheckHaveType(model.OtherType))
            {
                TempData["message"] = "類別名稱已存在，請輸入其它名稱或直接選擇該類別";
            }
            else
            {
                if (model.Data.Type == Guid.Empty)
                {
                    VacanciesType newtype = new VacanciesType();
                    newtype.Name = model.OtherType;
                    newtype.Createdby = CurrentUser.Employee.ID;
                    model.Data.Type = Services.GetService<VacanciesTypeService>().Create(newtype);
                    //model.Data.Type ;
                }
                model.Data.IsDeleted = false;
                int IsSuccess = Services.GetService<VacanciesService>().Create(model.Data, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "成功新增";
                    // ModelState.Clear();
                    WriteLog("Success:" + model.Data.Id);
                    return Json(new { success = true });
                }
            }
            return PartialView("_CreateFAQ", model);
        }

        public ActionResult Edit(Guid id)
        {
            SetBaseUserInfo();
            VacanciesViewModel viewModel = new VacanciesViewModel();
            List<VacanciesType> typeModel = Services.GetService<VacanciesTypeService>().GetFAQTypeList().ToList();
            Vacancies FAQModel = Services.GetService<VacanciesService>().GetFAQByID(id); ;
            viewModel.FAQTypeLists = typeModel;
            viewModel.Data = FAQModel;
            return PartialView("_EditFAQ", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(VacanciesViewModel model)
        {
            SetBaseUserInfo();
            List<VacanciesType> typeModel = Services.GetService<VacanciesTypeService>().GetFAQTypeList().ToList();
            model.FAQTypeLists = typeModel;
            if (!ModelState.IsValid)
            {
                //WriteLog("驗證失敗請檢查頁面資料");
                return PartialView("_EditFAQ", model);
            }
            else if (model.Data.Type == Guid.Empty && string.IsNullOrWhiteSpace(model.OtherType))
            {
                TempData["message"] = "請輸入其他類別名稱";
                //WriteLog("Other type is null in 驗證失敗請檢查頁面資料 ");
            }
            else
            {
                if (model.Data.Type == Guid.Empty)
                {
                    VacanciesType newtype = new VacanciesType();
                    newtype.Name = model.OtherType;
                    newtype.Createdby = CurrentUser.Employee.ID;
                    model.Data.Type = Services.GetService<VacanciesTypeService>().Create(newtype);
                }
                Vacancies oldData = Services.GetService<VacanciesService>().GetFAQByID(model.Data.Id);
                int IsSuccess = Services.GetService<VacanciesService>().Update(oldData, model.Data);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "編輯成功";
                    // ModelState.Clear();
                    WriteLog("Success:" + model.Data.Id);
                    return Json(new { success = true });
                }
            }

            WriteLog("Error" + model.Data.Id);
            return PartialView("_EditFAQ", model);
        }

        [HttpPost]
        public ActionResult DeleteFAQ(Guid faqId)
        {
            Vacancies _faq = Services.GetService<VacanciesService>().GetFAQByID(faqId);
            if (_faq == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "資料不存在" });
            }
            else
            {
                _faq.IsDeleted = true;
                _faq.Deletedby = CurrentUser.EmployeeID;
                _faq.DeletedTime = DateTime.Now;
                try
                {
                    Services.GetService<VacanciesService>().Update(_faq);
                    Services.GetService<VacanciesService>().SaveChanges();
                    TempData["message"] = "刪除成功";
                    WriteLog("Success:" + faqId);

                    return Json(new AjaxResult() { status = "success", message = "刪除成功" });
                }
                catch
                {
                    return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
                }
            }
        }

    }
}