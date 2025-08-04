using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public enum FormStatus
    {
        TempOrEmpty = -1,
        Draft = 0,
        Signing = 1,
        Approved = 2,
        Send = 3,
    }
}
