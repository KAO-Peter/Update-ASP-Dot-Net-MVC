using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmployeeData
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string EmpName { get; set; }
        public string EmpNameEN { get; set; }
        public string EmpID { get; set; }
        public string Sex { get; set; }
        public DateTime AssumeDate { get; set; }
        public string Tel { get; set; }
        public string Mobile { get; set; }
        public string CompanyEmail { get; set; }
        public string Address { get; set; }
        public string RegisterAddress { get; set; }
        public string IDNumber { get; set; }
        public string RegisterTel { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyAddress { get; set; }
        public string EmergencyRelation { get; set; }
        public string EmergencyPhone { get; set; }
        public int ID { get; set; }
        public DateTime? LeaveDate { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
