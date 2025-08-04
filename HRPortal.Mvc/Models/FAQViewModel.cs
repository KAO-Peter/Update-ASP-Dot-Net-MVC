using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
   public class FAQViewModel
    {
        public List<FAQ> FAQLists { get; set; }
        public FAQ Data { get; set; }
        public List<FAQType> FAQTypeLists { get; set; }

        public string OtherType { get; set; }
    }
}
