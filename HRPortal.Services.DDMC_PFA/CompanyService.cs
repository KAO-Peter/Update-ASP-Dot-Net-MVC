using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;

namespace HRPortal.Services.DDMC_PFA
{
    public class CompanyService : BaseCrudService<PfaCompany>
    {
        public CompanyService(HRPortal_Services services)
            : base(services)
        {
        }

        public List<PfaCompany> GetCompanyLists()
        {
            return GetAll().Where(x => x.Enabled == true).OrderBy(x=>x.CompanyCode).ToList();
        }
        public IEnumerable<PfaCompany> GetCompanyDate(Guid? ID)
        {
            return GetAll().Where(x => x.ID == ID);
        }
        public PfaCompany GetCompanyById(Guid id)
        {
            return GetAll().Where(x=>x.ID==id).FirstOrDefault();
        }
        public int Update(PfaCompany oldData, PfaCompany newData, bool isSave = true)
        {
            int result = 0;
            try
            {
                string[] updataproperties = { "ContactPrincipal"};
                result = this.Update(oldData, newData, updataproperties, isSave);
            }
            catch (Exception ex)
            {
                result = 0;

            }

            return result;
        }
        public override int Update(PfaCompany oldData, PfaCompany newData, string[] includeProperties, bool isSave = true)
        {
            int result = 0;
            try
            {
                isSave = true;
            }
            catch (Exception ex)
            {
                isSave = false;

            }
            result = base.Update(oldData, newData, includeProperties, isSave);

            return result;
        }
        public IEnumerable<ValueText> GetValueText()
        {
            //if (this.HasCacheData("GetValueText") == false)
            {
                IEnumerable<ValueText> data = GetAll().OrderBy(x => x.CompanyCode).Select(x => new ValueText { id = x.ID, v = x.ID.ToString(), t = x.CompanyCode + x.CompanyName, c = x.ContactPrincipal, originalCompanyName=x.CompanyName }).ToArray();
                //this.SetCacheData("GetValueText", data);
                return data;
            }
            //return this.GetCacheData("GetValueText");
        }

        /// <summary>
        /// 取得有效的公司資料，先依照 Sort 欄位排序，再依照 CompanyCode 排序。
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<PfaCompany> GetAllSort()
        {
            return this.Where(x => x.Enabled).OrderBy(x => x.CompanyCode);
        }
    }
}
