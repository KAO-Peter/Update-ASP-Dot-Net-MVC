using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class AnswerVacanciesService : BaseCrudService<AnswerVacancies>
    {
        public AnswerVacanciesService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<AnswerVacancies> GetAnswerFAQList()
        {
            IQueryable<AnswerVacancies> data = this.GetAll().Where(x => x.Status == 0).OrderByDescending(x => x.ModifiedTime);
            return data;
        }

        public AnswerVacancies GetAnswerFAQByID(Guid id)
        {
            AnswerVacancies data = this.GetAll().FirstOrDefault(x => x.ID == id);
            return data;
        }


        public override int Create(AnswerVacancies model, bool isSave = true)
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

        public override int Update(AnswerVacancies oldData, AnswerVacancies newData, string[] includeProperties, bool isSave = true)
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

        public int Update(AnswerVacancies oldData, AnswerVacancies newData, bool isSave = true)
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
