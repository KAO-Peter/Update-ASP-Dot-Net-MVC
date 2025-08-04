using System.Web.Mvc;

namespace HRPortal.Areas.FEDS
{
    public class FEDSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FEDS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FEDS_default",
                "FEDS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}