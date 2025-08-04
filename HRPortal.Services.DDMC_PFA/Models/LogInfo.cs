using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    public class LogInfo
    {
        public Guid? UserID { get; set; }
        public string UserIP { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
