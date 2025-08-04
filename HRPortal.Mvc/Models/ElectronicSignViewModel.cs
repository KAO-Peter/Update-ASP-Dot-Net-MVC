using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    /// <summary>
    /// 進出別
    /// </summary>
    public enum ClockInWayData : int
    {
        Work = 1,
        GetOffWork = 2
    }

    public enum ClockInReasonData : int
    {
        NormalCreditCard = 0,
        ForgotCreditCard = 1,
        ForgotBringCard = 2
    }
    public class ElectronicSignViewModel
    {
        public IEnumerable<SelectListItem> ClockInReason { get; set; }
        public IEnumerable<SelectListItem> ClockInWay { get; set; }
        public string ClockInReasonType { get; set; }
        public string ClockInWayType { get; set; }
        public DateTime DateTimeNow { get; set; }

    }
}
