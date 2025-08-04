using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaAbilityDetailService : BaseCrudService<PfaAbilityDetail>
    {
        public PfaAbilityDetailService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaAbilityDetail> GetAbilityDetail(Guid? id)
        {
            return Where(x => x.PfaAbilityID == id).OrderBy(x => x.Ordering).ToList();
        }
    }
}
