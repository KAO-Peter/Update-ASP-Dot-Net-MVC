//using HRPortal.Models;
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
    public class BambooHRTimeOffTypeService : BaseCrudService<BambooHRTimeOffType>
    {
        public BambooHRTimeOffTypeService(HRPortal_Services services)
            : base(services)
        {
        }

        public string GetTimeOffTypeIDByAbsentCode(string AbsentCode)
        {
            //目前先不管公司別，不會有兩家公司
            var item = GetAll().Where(x => x.HRM_AbsentCode == AbsentCode).FirstOrDefault();

            return item == null ? "" : item.BambooHR_TimeOffTypeID;
        }

       
    }
}
