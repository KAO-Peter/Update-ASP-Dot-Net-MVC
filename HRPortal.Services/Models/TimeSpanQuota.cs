using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities;

namespace HRPortal.Services.Models
{
    //簽核中假單的彙總資料
    public class TimeSpanPendingData
    {
        public Guid EmployeeID { get; set; }
        public string EmpID { get; set; }
        public string AbsentCode { get; set; }
        public DateTime TimeSpanBeginDate { get; set; }
        public DateTime TimeSpanEndDate { get; set; }
        public decimal AbsentHoursSum { get; set; }
        public decimal OriginalAmountSum { get; set; }
        public string AbsentUnit { get; set; }
        public List<LeaveForm> LeaveFormList { get; set; }
    }
}
