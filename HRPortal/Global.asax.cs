using HRPortal.ApiAdapter;
using HRPortal.Services;
using HRPortal.Services.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using YoungCloud.Configurations.AutofacSettings;
namespace HRPortal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AutofacInitializer.Configure(AutofacInitializer.InitializeRegister);

            //初始化APIUri
            HRMApiAdapter.GetHostUri = GetHostUri;
            HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapter.GetHostUri = GetHostUri;

            //20170328增加 by Daniel，切版判斷用的Session物件改為使用Global變數
            string siteType = System.Configuration.ConfigurationManager.AppSettings["SiteType"] ?? "";
            string siteFunctionRange = System.Configuration.ConfigurationManager.AppSettings["SiteFunctionRange"] ?? "";
            string allowedFunction = System.Configuration.ConfigurationManager.AppSettings["AllowedFunction"] ?? "";
            string allowedMenuID = System.Configuration.ConfigurationManager.AppSettings["AllowedMenuID"] ?? "";
            string mobileSitePath = System.Configuration.ConfigurationManager.AppSettings["MobileSitePath"] ?? "";

            SiteFunctionInfo.SiteType = siteType.ToLower();
            SiteFunctionInfo.SiteFunctionRange = siteFunctionRange.ToLower();
            SiteFunctionInfo.AllowedList = allowedFunction.Split(',').Select(p => p.ToLower().Trim()).ToList();
            SiteFunctionInfo.AllowedMenuID = allowedMenuID.Split(',').Select(p => p.ToLower().Trim()).ToList();
            SiteFunctionInfo.MobileSitePath = mobileSitePath;
            Application["ADLogin"] = false;
            if ((System.Configuration.ConfigurationManager.AppSettings["ADLogin"] ?? "false") == "true") Application["ADLogin"] = true;
        }

        //20170320增加 by Daniel，讀取切版資訊
        //20170324修改 by Daniel，選單檢查改為使用ID
        //20170328修改 by Daniel，改為使用Global變數，移除此段
        /*
        protected void Session_Start(object Sender, EventArgs e)
        {
            string siteType = System.Configuration.ConfigurationManager.AppSettings["SiteType"] ?? "";
            string siteFunctionRange = System.Configuration.ConfigurationManager.AppSettings["SiteFunctionRange"] ?? "";
            string allowedFunction = System.Configuration.ConfigurationManager.AppSettings["AllowedFunction"] ?? "";
            //string allowedMenu = System.Configuration.ConfigurationManager.AppSettings["AllowedMenu"] ?? "";
            string allowedMenuID = System.Configuration.ConfigurationManager.AppSettings["AllowedMenuID"] ?? "";

            Session["SiteType"] = siteType.ToLower();
            Session["SiteFunctionRange"] = siteFunctionRange.ToLower();
            Session["AllowedList"] = allowedFunction.Split(',').Select(p => p.ToLower().Trim()).ToList();
            //Session["AllowedMenu"] = allowedMenu.Split(',').Select(p => p.ToLower().Trim()).ToList();
            Session["AllowedMenuID"] = allowedMenuID.Split(',').Select(p => p.ToLower().Trim()).ToList();
        }
        */

        public static string GetHostUri()
        {
            using (HRPortal_Services _service = new HRPortal_Services())
            {
#if DEBUG
                return "http://10.2.2.30:8888/HRMWebAPI/";
                return "http://localhost:51232/";

#endif
                return _service.GetService<SystemSettingService>().GetSettingValue("HRMApiUri");
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //預設語系
            string _resourceName = "zh-cn";
            //取得用戶端語言設定
            string[] userLanguage = Request.UserLanguages;

            HttpCookie Lang = (HttpCookie) Request.Cookies["lang"];
            
            if (userLanguage != null && userLanguage[0] != null)
            {
                _resourceName = userLanguage[0];
            }
            if (Lang != null)
            {
                _resourceName = Lang.Value;
            }

            if (_resourceName == "zh-Hant-TW")
            {
                _resourceName = "zh-TW";
            }

            System.Threading.Thread.CurrentThread.CurrentCulture =
                 new CultureInfo(_resourceName);
            System.Threading.Thread.CurrentThread.CurrentUICulture =
             new CultureInfo(_resourceName);
        }


    }
}
