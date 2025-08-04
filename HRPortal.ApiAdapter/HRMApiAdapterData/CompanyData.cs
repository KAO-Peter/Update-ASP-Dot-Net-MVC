using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CompanyData
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool MainFlag { get; set; }
        public string CompanyEnglishName { get; set; }
    }
}
