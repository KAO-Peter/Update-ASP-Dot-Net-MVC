using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class BambooHRMailNotificationSettingService : BaseCrudService<BambooHRMailNotificationSetting>
    {
        public BambooHRMailNotificationSettingService(HRPortal_Services services)
            : base(services)
        {
        }

       
    }
}
