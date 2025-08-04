using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class VacanciesTypeService : BaseCrudService<VacanciesType>
    {
        public VacanciesTypeService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// VacanciesType列別列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<VacanciesType> GetFAQTypeList()
        {
            IQueryable<VacanciesType> data = this.GetAll().OrderByDescending(x => x.ModifiedTime);
            return data;
        }

        public bool CheckHaveType(string TypeName = "")
        {
            int _result = this.GetAll().Where(x => x.Name.Contains(TypeName)).ToList().Count;
            if (_result > 0)
                return true;
            else
                return false;
        }

        public Guid Create(VacanciesType model)
        {
            int result = 0;
            model.Id = Guid.NewGuid();
            model.CreatedTime = DateTime.Now;
            model.ModifiedTime = DateTime.Now;
            model.Modifiedby = model.Createdby;
            result = this.Create(model, true);
            if (result == 1)
            {
                return model.Id;
            }
            return Guid.Empty;
        }

        public override int Create(VacanciesType model, bool isSave = true)
        {
            try
            {
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }
    }
}
