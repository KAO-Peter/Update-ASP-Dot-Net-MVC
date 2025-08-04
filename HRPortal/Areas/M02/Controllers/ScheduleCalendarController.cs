using HRPortal.ApiAdapter;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using HRPortal.Mvc.Results;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HRPortal.ApiAdapter.HRMApiAdapterData;

namespace HRPortal.Areas.M02.Controllers
{
     public class ScheduleCalendarController : BaseController
    {
         public ActionResult Index()
         {
             return View();
         }
        [HttpPost]//抓取個人班表
        public async Task<ActionResult> EmpScheduleClassTime(DateTime startTime, DateTime endTime)
        {
            List<GetEmpScheduleClassTimeByStartEndTimeResponse> empScheduleTime = await HRMApiAdapter.GetEmpScheduleClassTimeByStartEndTime(CurrentUser.Employee.Company.CompanyCode, CurrentUser.EmployeeNO, startTime, endTime);

            return Json(empScheduleTime);
        }
       
    }
}