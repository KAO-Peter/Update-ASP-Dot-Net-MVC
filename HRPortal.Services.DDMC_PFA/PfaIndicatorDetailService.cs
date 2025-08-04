using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaIndicatorDetailService : BaseCrudService<PfaIndicatorDetail>
    {
        public PfaIndicatorDetailService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaIndicatorDetail> GetIndicatorDetail(Guid? id)
        {
            return Where(x => x.PfaIndicatorID == id).OrderBy(x => x.Ordering).ToList();
        }
    }
}