using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaOptionService : BaseCrudService<PfaOption>
    {
        public PfaOptionService(HRPortal_Services services)
            : base(services)
        {
        }

        public IList<PfaOption> GetPfaOption(Guid? optionGroupCode, bool isAdmin)
        {
            return Where(x => x.PfaOptionGroup.ID == optionGroupCode && x.PfaOptionGroup.IsAdmin == isAdmin).OrderBy(x => x.Ordering).ToList();
        }

        public IList<PfaOption> GetPfaOptionByGroupCode(string optionGroupCode, bool? isAdmin = null)
        {
            var query = Where(x => x.PfaOptionGroup.OptionGroupCode == optionGroupCode);
            if (isAdmin.HasValue)
                query = query.Where(x => x.PfaOptionGroup.IsAdmin == isAdmin);
            return query.OrderBy(x => x.Ordering).ToList();
        }

        public PfaOption GetPfaOption(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public bool IsExist(Guid? PfaOptionGroupID, string optionCode)
        {
            return Where(x => x.PfaOptionGroup.ID == PfaOptionGroupID && x.OptionCode == optionCode).Any();
        }
    }
}