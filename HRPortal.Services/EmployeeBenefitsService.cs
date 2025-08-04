//using HRPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity;

namespace HRPortal.Services
{
    public class EmployeeBenefitsService : BaseCrudService<EmployeeBenefits>
    {

        public EmployeeBenefitsService(HRPortal_Services services)
            : base(services)
        {
        }
        /// <summary>
        /// 公告列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<EmployeeBenefits> GetAnnounceList()
        {
            string key = "GetAnnounceList";

            IQueryable<EmployeeBenefits> data = this.GetAll().Where(x => x.IsDeleted == false && x.Status == true && (x.AnnounceEndTime == null || x.AnnounceEndTime >= DateTime.Now) && x.AnnounceStartTime <= DateTime.Now).OrderByDescending(x => x.IsSticky).ThenByDescending(x => x.AnnounceStartTime);

            return data;
        }

        public IQueryable<EmployeeBenefits> GetAnnounceAllList(string Status, string keyword)
        {
            IQueryable<EmployeeBenefits> data = this.GetAll().Where(x => x.IsDeleted == false).OrderByDescending(x => x.CreatedTime);
            if (!string.IsNullOrWhiteSpace(Status))
            {
                data = data.Where(x => x.Status == (Status == "0" ? false : true));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.Where(x => x.Title.Contains(keyword));
            }
            return data;

        }


        public EmployeeBenefits GetAnnounceByID(Guid id)
        {
            EmployeeBenefits data = GetAll().FirstOrDefault(x => x.Id == id);
            return data;
        }


        public override int Create(EmployeeBenefits model, bool isSave = true)
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


        public int Update(EmployeeBenefits oldData, EmployeeBenefits newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Title", "ContentText", "AnnounceStartTime", "AnnounceEndTime", "Status", "Modifiedby", "ModifiedTime", "IsSticky" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

        public bool UpdateStatus(bool ShowStatus, Guid Id, Guid ChangeModifiedbyId)
        {
            bool result = true;
            EmployeeBenefits oldData = this.GetAnnounceByID(Id);
            EmployeeBenefits newData = new EmployeeBenefits();
            newData.Modifiedby = ChangeModifiedbyId;
            string[] updataproperties = { "Status", "Modifiedby", "ModifiedTime" };
            if (ShowStatus)
            {
                newData.Status = true;
            }
            else
            {
                newData.Status = false;
            }

            try
            {
                Update(oldData, newData, updataproperties, true);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public override int Update(EmployeeBenefits oldData, EmployeeBenefits newData, string[] includeProperties, bool isSave = true)
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
            //if (result == 1)
            {
                //clear cache
                //this.ClearCacheData("GetAnnounceList");
            }
            return result;
        }

        public int Delete(EmployeeBenefits oldData, EmployeeBenefits newData, Guid employeeID, bool isSave = true)
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
