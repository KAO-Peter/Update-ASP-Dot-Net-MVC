using System.Web.Mvc;

namespace HRPortal.Areas.FEPH
{
    public class FEPHAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FEPH";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FEPH_default",
                "FEPH/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}