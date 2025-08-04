using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HRPortal.Services
{
    public class LeaveCancelService : BaseCrudService<LeaveCancel>
    {
        public LeaveCancelService(HRPortal_Services services)
            : base(services)
        {
        }

        public LeaveCancel GetLeaveCancelByLeaveFormID(string LeaveFormID)
        {
            LeaveCancel formdata= this.GetAll().Where(x => x.IsDeleted == false && x.LeaveForm.FormNo == LeaveFormID).FirstOrDefault();
            return formdata;
        }

        public LeaveCancel GetLeaveCancelByFormNo(string FormNo)
        {
            LeaveCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.FormNo == FormNo).FirstOrDefault();
            return formdata;
        }

        public LeaveCancel GetLeaveCancelByID(Guid Id)
        {
            LeaveCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.ID == Id).Include("LeaveForm").FirstOrDefault();
            return formdata;
        }
        public bool CheckCompleteByLeaveFormNo(string leaveFormNo,int status)
        {
            bool result = false;
            var _FormID = Services.GetService<LeaveFormService>().GetLeaveFormByFormNo(leaveFormNo);
            List<LeaveCancel> data = new List<LeaveCancel>();
            if (_FormID != null)
            {
                 data = this.GetAll().Where(x => x.IsDeleted == false && x.Status == status && x.LeaveFormID == _FormID.ID).ToList();
            }
            if (data.Count > 0)
            {
                result = true;
            }
            return result;
        }

        public int Create(LeaveCancel model)
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

        public int Update(LeaveCancel model)
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

        public override int Create(LeaveCancel model, bool isSave = true)
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

        public override int Update(LeaveCancel oldData, LeaveCancel newData, string[] includeProperties, bool isSave = true)
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

        public int Update(LeaveCancel oldData, LeaveCancel newData, bool isSave = true)
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

        public int Delete(LeaveCancel oldData, LeaveCancel newData, Guid employeeID, bool isSave = true)
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
