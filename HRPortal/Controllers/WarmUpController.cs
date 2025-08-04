using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using System.Threading.Tasks;
using HRPortal.Helper;
using HRPortal.Services;
using YoungCloud.SignFlow.Databases.Repositories;

namespace HRPortal.Controllers
{
    public class WarmUpController : Controller
    {
        protected HRPortal_Services Services;

        //
        // GET: /WarmUp/
        public async Task<ActionResult> Index()
        {
            
            string result = "";
            try
            {
               
                //Portal初始化Service(DBEntity應該也初始化了)
                Services = new HRPortal_Services();
                var model = Services.GetService<AnnouncementService>().GetAnnounceList();

                //SignFlow初始化DBEntity
                SignFlowRecRepository signFlowRecRepository = new SignFlowRecRepository();

                //API跑過一次
                AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2("ddmc", "admin", DateTime.Now, "remaining");
      
                //單獨執行Warm Up API
                result = await HRMApiAdapter.WakeUpHRMAPI();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
	}
}