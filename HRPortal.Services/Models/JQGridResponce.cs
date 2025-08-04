using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.Models
{
    public class JQGridResponce
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }
        public dynamic rows { get; set; }
    }
}
