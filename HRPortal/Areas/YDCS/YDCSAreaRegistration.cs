using System.Web.Mvc;

namespace HRPortal.Areas.YDCS
{
    public class YDCSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "YDCS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "YDCS_default",
                "YDCS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}