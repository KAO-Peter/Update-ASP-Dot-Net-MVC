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
    public class CasualCycleService : BaseCrudService<CasualCycle>
    {
        public CasualCycleService(HRPortal_Services services)
            : base(services)
        {
        }

        public override IQueryable<CasualCycle> GetAll()
        {
            return base.GetAll();
        }

        /// <summary>
        /// 查詢臨時工計薪
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CasualCycle GetByID(Guid companyID, int id)
        {
            return this.GetAll().Where(x => x.CompanyID == companyID && x.CasualCycle_ID == id).FirstOrDefault();
        }

        public override int Create(CasualCycle model, bool isSave = true)
        {
            try
            {
                model.ID = Guid.NewGuid();
                return base.Create(model, isSave);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public override int Update(CasualCycle model, bool isSave = true)
        {
            return base.Update(model, isSave);
        }

        public override int Delete(CasualCycle model, bool isSave = true)
        {
            return base.Delete(model, isSave);
        }
    }
}