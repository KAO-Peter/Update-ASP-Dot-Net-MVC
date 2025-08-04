using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using System.Threading.Tasks;

namespace HRPortal.Areas.FEDS.Controllers
{
    public class VacanciesQuestionController : BaseController
    {
        // GET: FEDS/AnswerVacancies
        public ActionResult Index(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            var ds = Services.GetService<AnswerVacanciesService>().GetAnswerFAQList();
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        public ActionResult Detail(Guid id)
        {
            SetBaseUserInfo();
            VacanciesQuestionViewModel viewmodel = new VacanciesQuestionViewModel();
            AnswerVacancies model = new AnswerVacancies();
            model = Services.GetService<AnswerVacanciesService>().GetAnswerFAQByID(id);
            viewmodel.AnswerData = model;
            return PartialView("_Detail", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReply(VacanciesQuestionViewModel viewmodel)
        {
            SetBaseUserInfo();
            AnswerVacancies olddata = Services.GetService<AnswerVacanciesService>().GetAnswerFAQByID(viewmodel.AnswerData.ID);
            viewmodel.AnswerData = olddata;
            if (!ModelState.IsValid)
            {
                return PartialView("_Detail", viewmodel);
            }
            else
            {
                viewmodel.AnswerData.Reply = viewmodel.ReplyData;
                viewmodel.AnswerData.ReplyID = CurrentUser.Employee.ID;
                int IsSuccess = Services.GetService<AnswerVacanciesService>().Update(olddata, viewmodel.AnswerData, true);
                if (IsSuccess == 1)
                {
                    TempData["message"] = "成功";
                    WriteLog("Success-sendmaill:" + viewmodel.AnswerData.Employee.Email);
                    SendMail(new string[] { viewmodel.AnswerData.Employee.Email },
                   new string[] { CurrentUser.Employee.Email }, new string[] { }, "問題回覆:" + viewmodel.AnswerData.Title, string.Format("詢問內容:{0}<br>================================<br>回覆內容:{1}<br><br>回覆者:【{2}】{3}<br>", viewmodel.AnswerData.ContentText.Replace("\r\n", "<br/>"), viewmodel.AnswerData.Reply.Replace("\r\n", "<br/>"), CurrentUser.Employee.Department.DepartmentName, CurrentUser.Employee.EmployeeName), true);
                    return Json(new { success = true });
                }
            }
            return PartialView("_Detail", viewmodel);
        }
    }
}