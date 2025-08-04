using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{

    public class DepartmentLeaveSummaryItem
    {
        public string DeptName { get; set; }
        public List<DepartmentMemberForLeaveSummary> DeptMember { get; set; }
    }

    public class DepartmentMemberForLeaveSummary
    {
        /// <summary>
        /// 員工代碼
        /// </summary>
        public string EmpID { get; set; }
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }
        /// <summary>
        /// 假別名稱
        /// </summary>
        public string AbsentName { get; set; }
        /// <summary>
        /// 假別英文名稱
        /// </summary>
        public string AbsentNameEN { get; set; }
        /// <summary>
        /// 開始時間
        /// </summary>
        public string BeginTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 總數
        /// </summary>
        public double AbsentAmount { get; set; }
        /// <summary>
        /// 單位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 表單編號(請假)
        /// </summary>
        public string FormNo { get; set; }
    }
}
