using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Model
{
    public class SignFlowFormLevelModel
    {
        public string FormLevelID { get; set; }
        public string LevelID { get; set; }
        public string FormType { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> DeptType { get; set; }
        public string SignDeptID { get; set; }
        public string DefalutSignerID { get; set; }
        public string IsUsed { get; set; }
        public string IsAddSigner { get; set; }
        public string Display { get; set; }
    }
}
