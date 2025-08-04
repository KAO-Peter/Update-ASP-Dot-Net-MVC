using HRPortal.DBEntities;

namespace HRPortal.Mvc.Models
{
    //假別單位
    public enum AbsentUnit : int
    {
        day=0,
        hour=1
    }

    public class LeaveFormViewModel
    {
        public string getLanguageCookie { get; set; }
        public LeaveForm FormData { get; set; }
        public AbsentUnit AbsentUnitData { get; set; }
        public bool IsChecked { get; set; }
        public decimal IsCheckedAmount { get; set; }
        public bool ChkLeave { get; set; }
    }
}
