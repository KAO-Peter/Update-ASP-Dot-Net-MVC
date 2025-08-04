//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HRPortal.Services.Models.BambooHR;

namespace HRPortal.Services
{
    public class BambooHRLeaveFormRecordLogService : BaseCrudService<BambooHRLeaveFormRecordLog>
    {
        public BambooHRLeaveFormRecordLogService(HRPortal_Services services)
            : base(services)
        {
        }

    }
}
