using System.Linq;
using HRPortal.DBEntities.DDMC_PFA;

namespace HRPortal.Services.DDMC_PFA
{
    public class MailAccountService : BaseCrudService<PfaMailAccount>
    {
        public MailAccountService(HRPortal_Services services)
            : base(services)
        {
        }

        public PfaMailAccount GetMailAccount(string mailAddress)
        {
            return GetAll().FirstOrDefault(x => x.MailAddress == mailAddress);
        }
    }
}
