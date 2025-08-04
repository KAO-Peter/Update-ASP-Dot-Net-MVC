using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HRPortal.DBEntities;
//using HRPortal.Models;
//using HRPortal.Models.CourseModels;
//using HRPortal.Services.CourseServices;
using System.Linq.Expressions;

namespace HRPortal.Services
{
    public class CasualFormService : BaseCrudService<CasualForm>
    {
        public CasualFormService(HRPortal_Services services)
            : base(services)
        {
        }

        public override IQueryable<CasualForm> GetAll()
        {
            return base.GetAll();
        }

        /// <summary>
        /// 查詢臨時工工時列表
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="DeptCode"></param>
        /// <returns></returns>
        public IQueryable<CasualForm> GetLists(Guid CompanyID, string DeptCode = "")
        {
            IQueryable<CasualForm> data = this.GetAll().Where(x => x.CompanyID == CompanyID);

            if (!String.IsNullOrEmpty(DeptCode))
            {
                data = data.Where(x => x.Dept_Code == DeptCode);
            }

            return data;
        }

        /// <summary>
        /// 查詢臨時工工時列表
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="DeptID"></param>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public IQueryable<CasualForm> GetListByEmpID(Guid CompanyID, int EmpID)
        {
            return this.GetAll().Where(x => x.CompanyID == CompanyID && x.EmpData_ID == EmpID);
        }

        /// <summary>
        /// 查詢臨時工工時
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CasualForm GetByID(Guid ID)
        {
            return this.GetAll().Where(x => x.ID == ID).FirstOrDefault();
        }

        /// <summary>
        /// 查詢臨時工工時列表
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="DeptCode"></param>
        /// <param name="ExcuteDate"></param>
        /// <param name="TimeType"></param>
        /// <param name="EmpID"></param>
        /// <param name="EmpName"></param>
        /// <returns></returns>
        public List<CasualForm> GetCasualFormLists(Guid CompanyID, DateTime ExcuteDate, string TimeType = "", string DeptCode = "", string EmpID = "", string EmpName = "")
        {
            IQueryable<CasualForm> _casualForm = GetAll().Where(x => x.CompanyID == CompanyID && x.ExcuteDate == ExcuteDate);

            switch (TimeType)
            { 
                case "1":  //已輸
                    _casualForm = _casualForm.Where(x => x.StartTime.ToString() != "" || x.EndTime.ToString() != "");
                    break;
                case "2":  //未輸
                    _casualForm = _casualForm.Where(x => x.StartTime.ToString() == "" || x.EndTime.ToString() == "");
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrEmpty(DeptCode))
            {
                _casualForm = _casualForm.Where(x => x.Dept_Code == DeptCode);
            }

            if (!String.IsNullOrEmpty(EmpID))
            {
                _casualForm = _casualForm.Where(x => x.EmployeeNO.Contains(EmpID));
            }

            if (!String.IsNullOrEmpty(EmpName))
            {
                _casualForm = _casualForm.Where(x => x.EmpName.Contains(EmpName));
            }

            //return _casualForm.OrderBy(x => x.CompanyID).ThenBy(x => x.Dept_Code).ThenBy(x => x.EmployeeNO).ToList();
            
            
            //20170829 註解原來的
            //return _casualForm.OrderBy(x => x.CompanyID).ThenBy(x => x.EmployeeNO).ThenBy(x => x.DefaultStartTime).ToList();

            //20170829 Daniel 調整排序，因Portal臨時工處理轉PDF時，會自動將同員工同一天第二筆以後的都歸零，改成按照計費順序邏輯排序(實際上班時間)
            return _casualForm.OrderBy(x => x.CompanyID).ThenBy(x => x.EmployeeNO).ThenBy(x => x.ExcuteDate).ThenBy(x => x.StartTime).ToList();
        }

        public override int Create(CasualForm model, bool isSave = true)
        {
            try
            {
                if (model.ID == null || model.ID == default(Guid))
                {
                    model.ID = Guid.NewGuid();
                }

                return base.Create(model, isSave);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int Create(ref Guid? id, CasualForm model, bool isSave = true)
        {
            try
            {
                if (model.ID == null || model.ID == default(Guid))
                {
                    model.ID = Guid.NewGuid();
                }

                id = model.ID;
                return base.Create(model, isSave);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public override int Update(CasualForm model, bool isSave = true)
        {
            //CacheData(model.ID);
            return base.Update(model, isSave);
        }

        public override int Delete(CasualForm model, bool isSave = true)
        {
            //CacheData(model.ID);
            return base.Delete(model, isSave);
        }
    }
}