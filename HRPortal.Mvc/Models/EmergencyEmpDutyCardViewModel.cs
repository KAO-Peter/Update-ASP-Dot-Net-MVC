using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class EmergencyEmpDutyCardViewModel
    {
        public string dutyCardTime { get; set; }

        public string cardNo { get; set; }

        public string empID { get; set; }

        public string empDeptCode { get; set; }

        public string empDeptName { get; set; }

        public string empName { get; set; }

        public string empNameEN { get; set; }
    }
}
