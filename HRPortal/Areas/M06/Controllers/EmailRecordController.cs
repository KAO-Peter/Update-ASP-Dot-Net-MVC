using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.DBEntities;

namespace HRPortal.Areas.M06.Controllers
{
    public class EmailRecordController : BaseController
    {
        //
        // GET: /M06/EmailRecord/
        public ActionResult Index(int page = 1, string BeginDate = "", string EndDate = "")
        {
            //Default
            GetDefaultData(BeginDate, EndDate);
            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(BeginDate) || string.IsNullOrWhiteSpace(EndDate))
                return View();
            //Get Mail Recode
            DateTime startTime = DateTime.Parse(BeginDate);
            DateTime endTime = DateTime.Parse(EndDate).AddDays(1);
            var ds = Services.GetService<MailMessageService>().Where(x => x.CreateTime >= startTime && x.CreateTime <= endTime).OrderByDescending(x=>x.CreateTime).ThenBy(x=>x.SendTimeStamp).ToList();
            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string btnQuery, string btnClear, int page = 1, string BeginDate = "", string EndDate = "")
        {
            if (!string.IsNullOrWhiteSpace(btnQuery) && (string.IsNullOrWhiteSpace(BeginDate) || string.IsNullOrWhiteSpace(EndDate)))
            {
                TempData["message"] = "起訖日期不能為空白";
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                page = 1;
                return RedirectToAction("Index", new
                {
                    page,
                    BeginDate,
                    EndDate
                });
            }
            //重整
            GetDefaultData(BeginDate, EndDate);
            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="employeedata"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        private void GetDefaultData(string starttime = "", string endtime = "")
        {
            DateTime dateNow = DateTime.Now.AddDays(1);
            ViewBag.StartTime = string.IsNullOrWhiteSpace(starttime) ? DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd") : starttime;
            ViewBag.EndTime = string.IsNullOrWhiteSpace(endtime) ? DateTime.Now.ToString("yyyy/MM/dd") : endtime;
        }

        public ActionResult Detail(Guid id)
        {
            MailMessage model = new MailMessage();
            model = Services.GetService<MailMessageService>().GetMailMessageByID(id);
            return PartialView("_Detail", model);
        }
    }
}