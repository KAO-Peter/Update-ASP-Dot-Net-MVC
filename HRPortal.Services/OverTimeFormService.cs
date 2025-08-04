using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HRPortal.Services
{
    public class OverTimeFormService : BaseCrudService<OverTimeForm>
    {
        public OverTimeFormService(HRPortal_Services services)
            : base(services)
        {
        }

        public OverTimeForm GetOverTimeFormByID(Guid id)
        {
            OverTimeForm data = Db.OverTimeForms.FirstOrDefault(x => x.ID == id);
            return data;
        }

        public OverTimeForm GetOverTimeFormByFormNo(string formNo)
        {
            OverTimeForm data = Db.OverTimeForms.FirstOrDefault(x => x.FormNo == formNo);
            return data;
        }

        public int Create(OverTimeForm model, HttpPostedFileBase filedata)
        {
            int result = 0;
            try
            {
                //upload file
                if (filedata != null && filedata.ContentLength > 0)
                {
                    try
                    {
                        string savePath = Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormFiles");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filedata.FileName);
                        var path = Path.Combine(HttpContext.Current.Server.MapPath(savePath), fileName);
                        filedata.SaveAs(path);
                        model.FilePath = fileName;
                        model.FileName = filedata.FileName;
                        model.FileFormat = filedata.ContentType;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }
                result = this.Create(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public override int Create(OverTimeForm model, bool isSave = true)
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

        public override int Update(OverTimeForm oldData, OverTimeForm newData, string[] includeProperties, bool isSave = true)
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

        public int Update(OverTimeForm oldData, OverTimeForm newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Status", "StartTime", "EndTime", "OverTimeAmount", "OverTimeReasonCode", "OverTimeReason", "CompensationWay", "HaveDinning", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

        public int Updates(OverTimeForm model, HttpPostedFileBase filedata)
        {
            int result = 0;
            try
            {
                //upload file
                if (filedata != null && filedata.ContentLength > 0)
                {
                    try
                    {
                        string savePath = Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormFiles");
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(filedata.FileName);
                        var path = Path.Combine(HttpContext.Current.Server.MapPath(savePath), fileName);
                        filedata.SaveAs(path);
                        model.FilePath = fileName;
                        model.FileName = filedata.FileName;
                        model.FileFormat = filedata.ContentType;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }

                result = this.Update(model, true);
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }


        public int Delete(OverTimeForm oldData, OverTimeForm newData, Guid employeeID, bool isSave = true)
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

        /// <summary>
        /// check data 是否在區間已存在
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool CheckOverTimeFormExist(DateTime startTime,DateTime endTime,Guid employeeID)
        {
            List<Guid> overTimeFormIdList = this.GetAll().Where(x => x.IsDeleted == false && x.Status > 0 && x.EmployeeID == employeeID
               && ((x.StartTime >= startTime && x.StartTime < endTime)
               || (startTime >= x.StartTime && startTime < x.EndTime))).Select(x => x.ID).ToList();
            overTimeFormIdList.RemoveAll(x => Services.GetService<OverTimeCancelService>().Any(y => y.OverTimeFormID == x && y.Status == 3 && !y.IsDeleted));
            if (overTimeFormIdList.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 取得以加班單起始日為基準當月已申請，尚未核准的加班單時數。
        /// </summary>
        /// <param name="employeeID">員工ID</param>
        /// <param name="BeginDate">加班單起始日</param>
        /// <returns></returns>
        public double GetInProcessingAmtByMonth(Guid employeeID, DateTime BeginDate)
        {
            DateTime bDate = new DateTime(BeginDate.Year, BeginDate.Month, 1);
            DateTime eDate = bDate.AddMonths(1);

            decimal amt = this.GetAll().Where(x => x.IsDeleted == false &&
                x.Status == 1 &&
                x.EmployeeID == employeeID &&
                x.StartTime >= bDate && x.StartTime <= eDate).Select(s => s.OverTimeAmount).DefaultIfEmpty(0m).Sum();

            return Convert.ToDouble(amt);
        }

    }
}
