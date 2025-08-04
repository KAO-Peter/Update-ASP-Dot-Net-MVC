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
    public class OptionGroupService : BaseCrudService<OptionGroup>
    {
        public OptionGroupService(HRPortal_Services services)
            : base(services)
        {
        }
    }
}
