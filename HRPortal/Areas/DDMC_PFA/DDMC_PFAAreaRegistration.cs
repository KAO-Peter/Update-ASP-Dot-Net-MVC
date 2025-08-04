using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA
{
    public class DDMC_PFAAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DDMC_PFA";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DDMC_PFA_default",
                "DDMC_PFA/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}