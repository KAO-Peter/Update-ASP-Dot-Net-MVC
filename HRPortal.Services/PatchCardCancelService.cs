using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace HRPortal.Services
{

    public class PatchCardCancelService : BaseCrudService<PatchCardCancel>
    {
        public PatchCardCancelService(HRPortal_Services services)
            : base(services)
        {
        }

        public PatchCardCancel GetOverTimeCancelByOverTimeFormID(string OverTimeFormID)
        {
            PatchCardCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.PatchCardForm.FormNo == OverTimeFormID).FirstOrDefault();
            return formdata;
        }

        public bool CheckCompleteByOverTimeFormNo(string overTimeFormNo, int status)
        {
            bool result = false;
            var _FormID = Services.GetService<PatchCardFormService>().GetOverTimeFormByFormNo(overTimeFormNo);
            List<PatchCardCancel> data = new List<PatchCardCancel>();
            if (_FormID != null)
            {
                data = this.GetAll().Where(x => x.IsDeleted == false && x.Status == status && x.PatchCardFormID == _FormID.ID).ToList();
            }
            if (data.Count > 0)
            {
                result = true;
            }
            return result;
        }

        public PatchCardCancel GetOverTimeCancelByFormNo(string FormNo)
        {
            PatchCardCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.FormNo == FormNo).FirstOrDefault();
            return formdata;
        }

        public PatchCardCancel GetOverTimeCancelByID(Guid Id)
        {
            PatchCardCancel formdata = this.GetAll().Where(x => x.IsDeleted == false && x.ID == Id).Include("PatchCardForm").FirstOrDefault();
            return formdata;
        }

        public int Create(PatchCardCancel model)
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

        public int Update(PatchCardCancel model)
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

        public override int Create(PatchCardCancel model, bool isSave = true)
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

        public override int Update(PatchCardCancel oldData, PatchCardCancel newData, string[] includeProperties, bool isSave = true)
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

        public int Update(PatchCardCancel oldData, PatchCardCancel newData, bool isSave = true)
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

        public int Delete(PatchCardCancel oldData, PatchCardCancel newData, Guid employeeID, bool isSave = true)
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
