using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
   public class LeaveCancelFormViewModel
    {
        public LeaveDisplayModel SourceData { get; set; }

        public LeaveCancel FormData { get; set; }
    }
}
