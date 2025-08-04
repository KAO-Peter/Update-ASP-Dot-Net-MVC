using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using System.Threading.Tasks;
using HRPortal.Mvc.Results;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.MultiLanguage;

namespace HRPortal.Areas.M02.Controllers
{
    public class ElectronicSignController : BaseController
    {
        //
        // GET: /M02/ElectronicSign/
        public ActionResult Index()
        {
            SetBaseUserInfo();
            ElectronicSignViewModel viewmodel = new ElectronicSignViewModel();
            viewmodel.ClockInReason = GetClockInReason();
            viewmodel.ClockInWay = GetClockInWay();

            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ElectronicSignViewModel viewmodel)
        {
            RequestResult _result = await HRMApiAdapter.PostDuty_FEDS(CurrentUser.Employee.Company.CompanyCode, CurrentUser.EmployeeNO, viewmodel.DateTimeNow, int.Parse(viewmodel.ClockInWayType), int.Parse(viewmodel.ClockInReasonType));
            if (_result.Status)
            {
               WriteLog("Success:"+ CurrentUser.EmployeeNO+"-" + viewmodel.DateTimeNow);
                return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = _result.Message });

            }
        }


        [HttpPost]
        public ActionResult CheckData(ElectronicSignViewModel model)
        {
            return Json(new AjaxResult()
            {
                status = "success",
                message = string.Format(
                          "<dl class='dl-info dl-horizontal'><dt>進出別</dt><dd>{0}</dd><dt>補卡原因</dt><dd>{1}</dd><dt>申請時間</dt><dd>{2}</dd></dl>",
                         GetClockInWayName((ClockInWayData)int.Parse(model.ClockInWayType)),
                          GetClockInReasonName((ClockInReasonData)int.Parse(model.ClockInReasonType)),
                          model.DateTimeNow)
            }
              );
        }

        private SelectList GetClockInReason(string selected = "")
        {
            SelectList options = new SelectList(new List<SelectListItem>
         {
             new SelectListItem { Text = GetClockInReasonName(ClockInReasonData.NormalCreditCard), Value = ((int)ClockInReasonData.NormalCreditCard).ToString(),Selected=(selected==((int)ClockInReasonData.NormalCreditCard).ToString()?true:false)},
             new SelectListItem { Text =GetClockInReasonName(ClockInReasonData.ForgotBringCard), Value =  ((int)ClockInReasonData.ForgotBringCard).ToString(),Selected=(selected==((int)ClockInReasonData.ForgotBringCard).ToString()?true:false)},
             new SelectListItem { Text = GetClockInReasonName(ClockInReasonData.ForgotCreditCard), Value =  ((int)ClockInReasonData.ForgotCreditCard).ToString(),Selected=(selected==((int)ClockInReasonData.ForgotCreditCard).ToString()?true:false)}
        }, "Value", "Text");
            return options;
        }

        private SelectList GetClockInWay(string selected="")
        {
          SelectList options= new SelectList(new List<SelectListItem>
         {
             new SelectListItem { Text = GetClockInWayName(ClockInWayData.Work), Value = ((int)ClockInWayData.Work).ToString(),Selected=(selected==((int)ClockInWayData.Work).ToString()?true:false)},
             new SelectListItem { Text = GetClockInWayName(ClockInWayData.GetOffWork), Value =  ((int)ClockInWayData.GetOffWork).ToString(),Selected=(selected==((int)ClockInWayData.GetOffWork).ToString()?true:false)}
        }, "Value", "Text");
            return options;
        }

        private string GetClockInReasonName(ClockInReasonData CodeData)
        {
            string _result = "";
            switch (CodeData)
            {
                case ClockInReasonData.NormalCreditCard:
                    _result = "正常打卡";
                    break;
                case ClockInReasonData.ForgotBringCard:
                    _result = "忘記帶卡";
                    break;
                case ClockInReasonData.ForgotCreditCard:
                    _result = "忘記刷卡";
                    break;
            }
            return _result; 
        }

        private string GetClockInWayName(ClockInWayData CodeData)
        {
            string _result = "";
            switch (CodeData)
            {
                case ClockInWayData.Work:
                    _result = "上班";
                    break;
                case ClockInWayData.GetOffWork:
                    _result = "下班";
                    break;
            }
            return _result;
        }


    }
}