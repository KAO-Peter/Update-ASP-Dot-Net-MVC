using System.Web.Mvc;

namespace HRPortal.Areas.M01
{
    public class M01AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "M01";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "M01_default",
                "M01/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}