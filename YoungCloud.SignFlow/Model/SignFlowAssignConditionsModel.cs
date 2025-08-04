using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Model
{
    public class SignFlowAssignConditionsModel
    {
        public Guid ID { get; set; }
        public string DesignID { get; set; }
        public string LevelID { get; set; }
        public string ConditionType { get; set; }
        public string Parameters { get; set; }
        public string IsUsed { get; set; }
    }
}
