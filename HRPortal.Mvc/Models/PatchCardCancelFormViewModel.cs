using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class PatchCardCancelFormViewModel
    {
        public PatchCardDisplayModel SourceData { get; set; }
        public PatchCardCancel FormData { get; set; }
    }
}
