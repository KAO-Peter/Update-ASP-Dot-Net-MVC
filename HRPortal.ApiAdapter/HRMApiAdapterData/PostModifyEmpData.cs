using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
     public class PostModifyEmpData
     {   //後臺名稱
         public string EmpID { get; set; }
         public string CompanyCode { get; set; }
         public string RegisterAddress { get; set; }
         public string Address { get; set; }
         public string Tel { get; set; }
         public string Mobile { get; set; }
         public string EmergencyName { get; set; }
         public string EmergencyPhone { get; set; }
         public string EmergencyAddress { get; set; }
         public string CompanyEmail { get; set; }
         public string RegisterTel { get; set; }
         public string EmergencyRelation { get; set; }
    }
}
