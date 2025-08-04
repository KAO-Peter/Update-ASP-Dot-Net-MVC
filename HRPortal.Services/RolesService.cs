using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class RoleService : BaseCrudService<Role>
    {
        public RoleService(HRPortal_Services services)
            : base(services)
        {
        }
    }
}
