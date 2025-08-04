using HRPortal.DBEntities.DDMC_PFA;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaOptionGroupService : BaseCrudService<PfaOptionGroup>
    {
        public PfaOptionGroupService(HRPortal_Services services)
            : base(services)
        {
        }

        public IList<PfaOptionGroup> GetPfaOptionGroup(bool isAdmin)
        {
            return Where(x => x.IsAdmin == isAdmin).OrderBy(x => x.Ordering).ToList();
        }
    }
}