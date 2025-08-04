using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HRPortal.Services
{
   
    public class OverTimeCancelService : BaseCrudService<OverTimeCancel>
    {
        public OverTimeCancelService(HRPortal_Services services)
            : base(services)
        {
        }

        public OverTimeCancel GetOverTimeCancelByOverTimeFormID(string OverTimeFormID)
        {
            OverTimeCancel formdata = this.GetAll().Where(x => x.IsDeleted==false && x.OverTimeForm.FormNo == OverTimeFormID).FirstOrDefault();
            return formdata;
        }

        public bool CheckCompleteByOverTimeFormNo(string overTimeFormNo, int status)
        {
            bool result = false;
            var _FormID = Services.GetService<OverTimeFormService>().GetOverTimeFormByFormNo(overTimeFormNo);
            List<OverTimeCancel> data = new List<OverTimeCancel>();
            if (_FormID != null)
            {
                data = this.GetAll().Where(x => x.IsDeleted == false && x.Status == status && x.OverTimeFormID == _FormID.ID).ToList();
            }
            if (data.Count > 0)
            {
                result = true;
            }
            return result;
        }

        public OverTimeCancel GetOverTimeCancelByFormNo(string FormNo)
        {
            OverTimeCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.FormNo == FormNo).FirstOrDefault();
            return formdata;
        }

        public OverTimeCancel GetOverTimeCancelByID(Guid Id)
        {
            OverTimeCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.ID == Id).Include("OverTimeForm").FirstOrDefault();
            return formdata;
        }

        public int Create(OverTimeCancel model)
        {
            int result = 0;
            try
            {
                result = this.Create(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public int Update(OverTimeCancel model)
        {
            int result = 0;
            try
            {

                result = this.Update(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public override int Create(OverTimeCancel model, bool isSave = true)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                model.ModifiedTime = DateTime.Now;
                model.Modifiedby = model.Createdby;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }

        public override int Update(OverTimeCancel oldData, OverTimeCancel newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.ModifiedTime = DateTime.Now;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;

            }
            result = base.Update(oldData, newData, includeProperties, isSave);

            return result;
        }

        public int Update(OverTimeCancel oldData, OverTimeCancel newData, bool isSave = true)
        {
            int result = 0;
            try
            {

                string[] updataproperties = { "Status", "CancelReason", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

        public int Delete(OverTimeCancel oldData, OverTimeCancel newData, Guid employeeID, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.IsDeleted = true;
                newData.DeletedTime = DateTime.Now;
                newData.ModifiedTime = DateTime.Now;
                newData.Deletedby = employeeID;
                newData.Modifiedby = employeeID;
                string[] updataproperties = { "IsDeleted", "Deletedby", "DeletedTime", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }
    }
}
