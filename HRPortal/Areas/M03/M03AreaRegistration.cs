using System.Web.Mvc;

namespace HRPortal.Areas.M03
{
    public class M03AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "M03";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "M03_default",
                "M03/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}