using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class UpdateRoleMenuModel
    {
        public Guid RoleID { get; set; }
        public Guid[] MenuList { get; set; }
    }
}
