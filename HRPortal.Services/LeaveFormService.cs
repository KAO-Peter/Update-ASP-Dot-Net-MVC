using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;


namespace HRPortal.Services
{
    public class LeaveFormService : BaseCrudService<LeaveForm>
    {
        public LeaveFormService(HRPortal_Services services)
            : base(services)
        {
        }

        public LeaveForm GetLeaveFormByID(Guid id)
        {
            LeaveForm data = Db.LeaveForms.FirstOrDefault(x => x.ID == id);
            return data;
        }

        public LeaveForm GetLeaveFormByIDWithEmployee(Guid id)
        {
            LeaveForm data = Db.LeaveForms.Include("Employee").Include("Department").FirstOrDefault(x => x.ID == id);
            return data;
        }

        public LeaveForm GetLeaveFormByFormNo(string formNo)
        {
            LeaveForm data = Db.LeaveForms.FirstOrDefault(x => x.FormNo == formNo);
            return data;
        }

        public LeaveForm GetLeaveFormByFormNoWithEmployee(string formNo)
        {
            LeaveForm data = Db.LeaveForms.Include("Employee").Include("Department").FirstOrDefault(x => x.FormNo == formNo);
            return data;
        }

        public int Create(LeaveForm model, HttpPostedFileBase filedata)
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

        public int Update(LeaveForm model, HttpPostedFileBase filedata)
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

        public override int Create(LeaveForm model, bool isSave = true)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                model.ModifiedTime = DateTime.Now;
                model.Modifiedby = model.Createdby;
                model.AfterAmount = model.AbsentAmount - model.LeaveAmount;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }

        public override int Update(LeaveForm oldData, LeaveForm newData, string[] includeProperties, bool isSave = true)
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

        public int Update(LeaveForm oldData, LeaveForm newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Status", "AbsentCode", "AbsentAmount", "StartTime", "EndTime", "LeaveAmount", "AfterAmount", "LeaveReason", "AgentID", "FilePath", "FileName", "Modifiedby", "ModifiedTime" };
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }

        public int Delete(LeaveForm oldData, LeaveForm newData, Guid employeeID, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.IsDeleted = true;
                newData.DeletedTime = DateTime.Now;
                newData.ModifiedTime = DateTime.Now;
                newData.Deletedby = employeeID;
                newData.Modifiedby = employeeID;
                string[] updataproperties = { "IsDeleted","Deletedby","DeletedTime", "Modifiedby", "ModifiedTime" };
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
        public bool CheckLeaveFormExist(DateTime startTime, DateTime endTime, Guid employeeID)
        {
            List<Guid> leaveFormIdList = this.GetAll().Where(x => x.IsDeleted == false && x.Status > 0 && x.EmployeeID == employeeID
                && ((x.StartTime >= startTime && x.StartTime < endTime)
                || (startTime >= x.StartTime && startTime < x.EndTime))).Select(x => x.ID).ToList();
            leaveFormIdList.RemoveAll(x => Services.GetService<LeaveCancelService>().Any(y => y.LeaveFormID == x && y.Status == 3 && !y.IsDeleted));
            if (leaveFormIdList.Count > 0)
                return true;
            else
                return false;
        }

        public Dictionary<string, decimal> SummaryNotApprovedAbsentAmount(Guid employeeID,DateTime selectDate)
        {
            /*
            return GetAll().Where(x => x.EmployeeID == employeeID && x.Status < 3 && !x.IsDeleted)
                .GroupBy(x => x.AbsentCode).ToDictionary(g => g.Key, g => g.Sum(x => x.LeaveAmount));
             */
            return GetAll().Where(x => x.EmployeeID == employeeID && x.Status > 0 && x.Status < 3 && (x.StartTime <= selectDate && x.EndTime < selectDate) && !x.IsDeleted)
                .GroupBy(x => x.AbsentCode).ToDictionary(g => g.Key, g => g.Sum(x => x.LeaveAmount));
            
        }

        //取得簽核中假單時數
        public List<TimeSpanPendingData> GetTimeSpanPendingLeaveData(QueryEmpHolidayTimeSpanQuotaObj QueryParams)
        {
            List<TimeSpanPendingData> result = new List<TimeSpanPendingData>();

            //時間有跨到就算
            var query = GetAll().Where(x => x.Company.CompanyCode == QueryParams.CompanyCode && x.Department.DepartmentCode == QueryParams.DepartmentCode
                                        && x.Status > 0 && x.Status < 3 && !x.IsDeleted);
                                        // && x.StartTime <= QueryParams.QueryEndDate && x.EndTime >= QueryParams.QueryBeginDate); //簽核中目前直接看全部的

            if (QueryParams.EmpIDList.Count > 0)
            {
                query = query.Where(x => QueryParams.EmpIDList.Contains(x.Employee.EmployeeNO));
            }

            if (QueryParams.AbsentCodeList.Count > 0)
            {
                query = query.Where(x => QueryParams.AbsentCodeList.Contains(x.AbsentCode));
            }

            result = query.GroupBy(x => new { x.EmployeeID, x.Employee.EmployeeNO, x.AbsentCode, x.AbsentUnit })
                    .Select(g => new TimeSpanPendingData()
                        {
                            EmployeeID = g.Key.EmployeeID,
                            EmpID = g.Key.EmployeeNO,
                            AbsentCode = g.Key.AbsentCode,
                            TimeSpanBeginDate = QueryParams.QueryBeginDate,
                            TimeSpanEndDate = QueryParams.QueryEndDate,
                            AbsentHoursSum = g.Sum(s => s.LeaveAmount * (s.AbsentUnit == "d" ? 8 : 1)),
                            OriginalAmountSum = g.Sum(s => s.LeaveAmount),
                            AbsentUnit = g.Key.AbsentUnit,
                            LeaveFormList = g.ToList()

                        }).OrderBy(o1 => o1.EmpID).ThenBy(o2 => o2.AbsentCode).ToList();

            return result;
        }

        //取得簽核中與已簽核(未銷假)假單資訊，含簽核起訖時間
        public List<LeaveForm> GetValidLeaveForm(QueryLeaveFormSignStatusObj QueryParams)
        {
            List<LeaveForm> result = new List<LeaveForm>();

            //時間有跨到就算，排除已經銷假的
            var query = GetAll().Where(x => x.Company.CompanyCode == QueryParams.CompanyCode && x.Department.DepartmentCode == QueryParams.DepartmentCode
                                        && x.StartTime <= QueryParams.QueryEndDate && x.EndTime >= QueryParams.QueryBeginDate
                                        && x.Status > 0 && x.Status <= 3 && !x.IsDeleted && !x.LeaveCancels.Any(y => y.Status == 3)).Include("Employee");
            
            if (QueryParams.EmpIDList.Count > 0)
            {
                query = query.Where(x => QueryParams.EmpIDList.Contains(x.Employee.EmployeeNO));
            }

            if (QueryParams.AbsentCodeList.Count > 0)
            {
                query = query.Where(x => QueryParams.AbsentCodeList.Contains(x.AbsentCode));
            }

            //已使用的，只算當下日期之前的假單，未來的，只算當下日期以及之後的假單
            DateTime dateNow = DateTime.Now.Date;
            query = QueryParams.isUsed ? query.Where(x => x.StartTime < dateNow) : query.Where(x => x.StartTime >= dateNow);

            //result = query.OrderBy(x => x.EmployeeID).ThenBy(y => y.StartTime).ThenBy(z => z.AbsentCode).ToList();
            result = query.ToList(); //先不排序，跟後台資料合併後再排

            return result;
        }
     }
}
