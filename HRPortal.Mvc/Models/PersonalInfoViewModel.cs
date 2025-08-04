using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class PersonalInfoViewModel
    {
        public IEnumerable<SelectListItem> signDepartmentLists { get; set; }

        public Employee employeeData { get; set; }
    }
}
