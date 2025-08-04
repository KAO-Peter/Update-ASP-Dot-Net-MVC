using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
   public class OverTimeCancelFormViewModel
    {
        public OverTimeDisplayModel SourceData { get; set; }
        public OverTimeCancel FormData { get; set; }
        public string LanguageCookie { get; set; }
    }
}
