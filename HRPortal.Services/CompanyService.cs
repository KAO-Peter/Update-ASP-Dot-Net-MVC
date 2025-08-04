//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class CompanyService : BaseCrudService<Company>
    {
        public CompanyService(HRPortal_Services services)
            : base(services)
        {
        }

        public List<Company> GetCompanyLists()
        {
            return GetAll().Where(x => x.Enabled == true).OrderBy(x=>x.CompanyCode).ToList();
        }
        public IEnumerable<Company> GetCompanyDate(Guid? ID)
        {
            return GetAll().Where(x => x.ID == ID);
        }
        public Company GetCompanyById(Guid id)
        {
            return GetAll().Where(x=>x.ID==id).FirstOrDefault();
        }
        public int Update(Company oldData, Company newData, bool isSave = true)
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
        public override int Update(Company oldData, Company newData, string[] includeProperties, bool isSave = true)
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
    }
}
