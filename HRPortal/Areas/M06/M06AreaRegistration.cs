using System.Web.Mvc;

namespace HRPortal.Areas.M06
{
    public class M06AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "M06";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "M06_default",
                "M06/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}