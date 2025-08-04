using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class FAQService : BaseCrudService<FAQ>
    {
        public FAQService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// FAQ列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<FAQ> GetFAQList()
        {
            IQueryable<FAQ> data = this.Where(x => !x.IsDeleted.Value).OrderByDescending(x => x.ModifiedTime);
            return data;
        }

        public FAQ GetFAQByID(Guid id)
        {
            FAQ data = Db.FAQs.FirstOrDefault(x => x.Id == id);
            return data;
        }

        public override int Create(FAQ model, bool isSave = true)
        {
            try
            {
                model.Id = Guid.NewGuid();
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

        public override int Update(FAQ oldData, FAQ newData, string[] includeProperties, bool isSave = true)
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

        public int Update(FAQ oldData, FAQ newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Title", "ContentText", "Type", "Modifiedby", "ModifiedTime" };
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
