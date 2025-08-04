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
    public class MailAccountService : BaseCrudService<MailAccount>
    {
        public MailAccountService(HRPortal_Services services)
            : base(services)
        {
        }

        public MailAccount GetMailAccount(string mailAddress)
        {
            return GetAll().FirstOrDefault(x => x.MailAddress == mailAddress);
        }
    }
}
