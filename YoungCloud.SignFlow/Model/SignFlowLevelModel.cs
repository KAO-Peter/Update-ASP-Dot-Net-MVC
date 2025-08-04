using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Model
{
   public class SignFlowLevelModel
    {
        public string LevelID { get; set; }
        public string LevelType { get; set; }
        public string Name { get; set; }
        public decimal LevelOrder { get; set; }
        public string IsUsed { get; set; }
        public string SearchDisplay { get; set; }
        public string SystemColumn { get; set; }
        public string AgentGroup { get; set; }
    }
}
