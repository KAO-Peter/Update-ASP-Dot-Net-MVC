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
    public class SmartSheetMappingService : BaseCrudService<SmartSheetMapping>
    {
        public SmartSheetMappingService(HRPortal_Services services)
            : base(services)
        {
        }

        public override int Create(SmartSheetMapping model, bool isSave = true)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }

        public string GetRowIDByLeaveFormID(Guid id)
        {
            var item = GetAll().Where(x => x.LeaveFormID == id).OrderByDescending(y => y.CreatedTime).FirstOrDefault();

            return item == null ? "" : item.RowID;
        }

        public int DeleteByLeaveFormID(Guid id)
        {
            var deleteList = GetAll().Where(x => x.LeaveFormID == id).ToList(); //該請假單ID對應的全部刪除，正常應該也只有一筆
            int result = 0;
            
            try
            {
                result = this.Delete(deleteList);
            }
            catch (Exception ex)
            {
 
            }

            return result;
        }
        
       
    }
}
