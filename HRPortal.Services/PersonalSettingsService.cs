using HRPortal.DBEntities;
using System.Collections.Generic;
using System.Linq;

namespace HRPortal.Services
{
    public class PersonalSettingsService : BaseCrudService<PersonalSettings>
    {
        public PersonalSettingsService(HRPortal_Services services)
            : base(services)
        {
        }

        /// <summary>取得使用者設定值</summary>
        /// <returns></returns>
        public List<PersonalSettings> GetParameter(string Type, string CompanyCode, string EmployeeNO)
        {
            return GetAll().Where(x => x.Type == Type &&
                                       x.CompanyCode == CompanyCode &&
                                       x.EmployeeNO == EmployeeNO
                                 ).ToList();
        }
    }
}