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
    public class CasualFormLogService : BaseCrudService<CasualFormLog>
    {
        public CasualFormLogService(HRPortal_Services services)
            : base(services)
        {
        }

        public override IQueryable<CasualFormLog> GetAll()
        {
            return base.GetAll();
        }

        public override int Create(CasualFormLog model, bool isSave = true)
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

        //public override int Update(CasualFormLog model, bool isSave = true)
        //{
        //    //CacheData(model.ID);
        //    return base.Update(model, isSave);
        //}

        //public override int Delete(CasualFormLog model, bool isSave = true)
        //{
        //    //CacheData(model.ID);
        //    return base.Delete(model, isSave);
        //}
    }
}