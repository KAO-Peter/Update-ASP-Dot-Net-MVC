using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Model
{
    public class SignFlowDesignModel
    {
        public System.Guid ID { get; set; }
        public string DesignID { get; set; }
        public string FormLevelID { get; set; }
        public Nullable<decimal> DeptType { get; set; }
        public string SignDeptID { get; set; }
        public string SignerID { get; set; }
        public decimal SignOrder { get; set; }
        public string SignCompanyID { get; set; }
    }
}
