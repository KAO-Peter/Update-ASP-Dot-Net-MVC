using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaPerformanceService : BaseCrudService<PfaPerformance>
    {
        public PfaPerformanceService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaPerformance> GetPfaPerformanceData(string txtCode, string txtName)
        {
            var result = GetAll();

            if (!string.IsNullOrEmpty(txtCode))
            {
                result = result.Where(x => x.Code.Contains(txtCode));
            }
            if (!string.IsNullOrEmpty(txtName))
            {
                result = result.Where(x => x.Name.Contains(txtName));
            }
            return result.OrderBy(x => x.Code).ToList();
        }

        public bool IsExist(string Code)
        {
            return Where(x => x.Code == Code).Any();
        }

        public bool IsOverLapping(Guid? id, decimal? ScoresStart, decimal? ScoresEnd)
        {
            return Where(x => x.ID != id && ((x.ScoresStart <= ScoresStart && x.ScoresEnd >= ScoresStart) || (x.ScoresStart <= ScoresEnd && x.ScoresEnd >= ScoresEnd))).Any();
        }

        public PfaPerformance GetPfaPerformance(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }
    }
}