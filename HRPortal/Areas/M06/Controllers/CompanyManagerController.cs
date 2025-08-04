using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Results;
namespace HRPortal.Areas.M06.Controllers
{
    public class CompanyManagerController : BaseController
    {
        // GET: M06/CompanyManager
        public ActionResult Index(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var ds = this.Services.GetService<CompanyService>().GetCompanyLists();
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        public ActionResult Detail(Guid id)
        {
            Company model = new Company();
            model = this.Services.GetService<CompanyService>().GetCompanyById(id);
            return PartialView("_Detail", model);
        }
        [HttpPost]
        public ActionResult Update(string CompanyCode,string ContactPrincipal)
        {
            var olddata = Services.GetService<CompanyService>().GetAll().Where(x => x.CompanyCode == CompanyCode).FirstOrDefault();
            
            var newdata = olddata;    
    
            newdata.ContactPrincipal = ContactPrincipal;

            var _result = Services.GetService<CompanyService>().Update(olddata, newdata, true);
            if (_result == 1)
            {
                
                return Json(new AjaxResult() { status = "success", message = "更新成功" });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
            }
           
        }
    }
}