using System.Web.Mvc;

namespace HRPortal.Areas.M02
{
    public class M02AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "M02";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "M02_default",
                "M02/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}