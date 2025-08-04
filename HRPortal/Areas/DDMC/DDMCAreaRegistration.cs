using System.Web.Mvc;

namespace HRPortal.Areas.DDMC
{
    public class DDMCAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "DDMC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "DDMC_default",
                "DDMC/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}