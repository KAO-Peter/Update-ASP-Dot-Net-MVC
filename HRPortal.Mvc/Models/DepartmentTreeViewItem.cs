using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class DepartmentTreeViewItem
    {
        public Guid ID { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public Nullable<Guid> ParentID { get; set; }
    }
}
