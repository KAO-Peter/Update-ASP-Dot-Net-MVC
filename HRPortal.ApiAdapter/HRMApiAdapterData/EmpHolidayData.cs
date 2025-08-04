using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmpHolidayData
    {
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }

        /// <summary>
        /// 員工英文姓名
        /// </summary>
        public string EmpNameEN { get; set; }

        /// <summary>
        /// 員工編號
        /// </summary>
        public string EmpID { get; set; }

        /// <summary>
        /// 假別代碼
        /// </summary>
        public string AbsentCode { get; set; }

        /// <summary>
        /// 假別名稱
        /// </summary>
        public string AbsentName { get; set; }

        /// <summary>
        /// 假別英文名稱
        /// </summary>
        public string AbsentNameEN { get; set; }

        /// <summary>
        /// 可用數
        /// </summary>
        public double CanUseCount { get; set; }

        /// <summary>
        /// 單位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 生效日
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 失效日
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
