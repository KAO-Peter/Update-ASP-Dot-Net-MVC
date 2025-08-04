using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class GetEmpScheduleClassTimeByStartEndTimeResponse : RequestResult
    {
        /// <summary>
        /// 班表類別名稱
        /// </summary>
        public String ClassName { get; set; }

        /// <summary>
        /// 班表名稱
        /// </summary>
        public String ScheduleName { get; set; }

        /// <summary>
        /// 排班開始日期
        /// </summary>
        public String StartDate { get; set; }

        /// <summary>
        /// 排班結束日期
        /// </summary>
        public String EndDate { get; set; }

        /// <summary>
        /// 排班開始時間
        /// </summary>
        public String StartTime { get; set; }

        /// <summary>
        /// 排班結束時間
        /// </summary>
        public String EndTime { get; set; }

        /// <summary>
        /// 假別
        /// </summary>
        public String TimetableName { get; set; }
    }
}
