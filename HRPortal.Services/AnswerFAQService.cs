using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class AnswerFAQService : BaseCrudService<AnswerFAQ>
    {
        public AnswerFAQService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<AnswerFAQ> GetAnswerFAQList()
        {
            IQueryable<AnswerFAQ> data = this.GetAll().Where(x=>x.Status==0).OrderByDescending(x => x.ModifiedTime);
            return data;
        }

        public AnswerFAQ GetAnswerFAQByID(Guid id)
        {
            AnswerFAQ data = this.GetAll().FirstOrDefault(x => x.ID == id);
            return data;
        }


        public override int Create(AnswerFAQ model, bool isSave = true)
        {
            try
            {
                model.CreatedTime = DateTime.Now;
                model.ModifiedTime = DateTime.Now;
                model.Modifiedby = model.Createdby;
                model.Status = 0;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;
            }
            return base.Create(model, isSave);
        }

        public override int Update(AnswerFAQ oldData, AnswerFAQ newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                newData.ModifiedTime = DateTime.Now;
                newData.Status = 1;
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;

            }
            result = base.Update(oldData, newData, includeProperties, isSave);

            return result;
        }

        public int Update(AnswerFAQ oldData, AnswerFAQ newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "Reply", "ReplyID", "Status", "Modifiedby", "ModifiedTime" };
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
