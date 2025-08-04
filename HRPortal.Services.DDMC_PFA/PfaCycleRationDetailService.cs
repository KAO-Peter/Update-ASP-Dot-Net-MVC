using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaCycleRationDetailService : BaseCrudService<PfaCycleRationDetail>
    {
        public PfaCycleRationDetailService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaCycleRationDetail> GetPfaCycleRationDetail(Guid? id)
        {
            return Where(x => x.PfaCycleRationID == id).OrderBy(x => x.Ordering).ThenBy(x => x.Code).ToList();
        }
    }
}
