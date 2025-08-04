using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualFormResult : RequestResult
    {
        public string CompanyCode { get; set; }
        public Nullable<System.Guid> CasualFormID { get; set; }
    }
}
