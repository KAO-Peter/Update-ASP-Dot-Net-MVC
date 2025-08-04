//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class OptionService : BaseCrudService<Option>
    {
        public OptionService(HRPortal_Services services)
            : base(services)
        {
        }

        public IList<Option> GetOptionListByGroup(string groupKey)
        {
            return Where(x => x.OptionGroup.OptionGroupKey == groupKey).OrderBy(x => x.OptionValue).ToList();
        }
    }
}
