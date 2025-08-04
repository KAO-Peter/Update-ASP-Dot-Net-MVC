using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class SignFlowConditionViewModel
    {
        public Guid ID { get; set; }
        public string DesignID { get; set; }
        public string LevelID { get; set; }
        public string LevelName { get; set; }
        public string ConditionType { get; set; }
        public string ConditionDisplayName { get; set; }
        public string Parameters { get; set; }
        public string AbsentCode { get; set; }
        public string AbsentName { get; set; }
    }
}
