using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.DBEntities;
using HRPortal.Services;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M06
{
    public class FormSettingController : BaseController
    {
        // GET: /M06/FormSettingController/Index
        public ActionResult Index()
        {
            List<Company> CompanyList = Services.GetService<CompanyService>().GetAll().ToList();

            List<SelectListItem> companyListItems = new List<SelectListItem>();

            foreach (Company item in CompanyList)
            {
                companyListItems.Add(new SelectListItem()
                    {
                        Text = item.CompanyName,
                        Value =item.CompanyCode
                    });
            }

            ViewBag.companyListItems = companyListItems;

            return View();
        }

        // GET: /M06/FormSettingController/CreateFormStatusPage
        public ActionResult CreateFormStatusPage(string CompanyCode=null) 
        {
            List<FormSetting> FormSettingList=new List<FormSetting>();
            if(CompanyCode==null)
            {
                FormSettingList = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.CompanyCode);
            }else
            {
                FormSettingList = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CompanyCode);
            }
            return View("_formSetting", FormSettingList);
        }

        // POST: /M06/FormSettingController/UpdateFormSettingStatus
        public ActionResult UpdateFormSettingStatus(List<FormSettingViewModel> model)
        {
            
            bool result = false;
            foreach (FormSettingViewModel ViewModel in model)
            {
                result=Services.GetService<FormSettingService>().UpdateStatus(ViewModel.SettingValue,ViewModel.FormID);
            }

            if (result == true) {
                TempData["message"] = "修改完成。";
            }
            else if (result == false) {
                TempData["message"] = "修改失敗。";
            }

            return RedirectToAction("Index");
        }
    }
}
