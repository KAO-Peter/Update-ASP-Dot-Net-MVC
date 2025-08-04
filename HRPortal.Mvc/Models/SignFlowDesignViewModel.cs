using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class SignFlowDesignViewModel
    {
        public Guid ID { get; set; }
        public string DesignID { get; set; }
        public string SignOrder { get; set; }
        public string FormLevelID { get; set; }
        public string FormLevelName { get; set; }
        public string DeptType { get; set; }
        public string DeptTypeName { get; set; }
        public string SignerID { get; set; }
        public string SignerName { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
    }
}
