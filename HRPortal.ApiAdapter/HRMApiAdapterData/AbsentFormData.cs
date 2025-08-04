using System;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class AbsentFormData
    {
        public DateTime CreateTime { get; set; }

        /// <summary> 流水號 </summary>
        public int ID { get; set; }

        /// <summary> 員工編號 </summary>
        public string EmpID { get; set; }

        /// <summary> 員工姓名 </summary>
        public string EmpName { get; set; }

        /// <summary> 部門編號 </summary>
        public string DeptCode { get; set; }

        /// <summary> 部門名稱 </summary>
        public string DeptName { get; set; }

        /// <summary> 開始時間 </summary>
        public DateTime BeginTime { get; set; }

        /// <summary> 結束時間 </summary>
        public DateTime EndTime { get; set; }

        /// <summary> 內容 </summary>
        public string Summary { get; set; }

        /// <summary> 假單編號 </summary>
        public string FormNo { get; set; }

        /// <summary> 員工姓名英文 </summary>
        public string EmpNameEN { get; set; }

        /// <summary> 代理人姓名 </summary>
        public string AgentName { get; set; }

        /// <summary> 代理人英文姓名 </summary>
        public string AgentNameEN { get; set; }

        /// <summary> 部門英文名稱 </summary>
        public string DeptNameEN { get; set; }

        /// <summary> 假別代碼 </summary>
        public string AbsentCode { get; set; }

        /// <summary> 請假時數 </summary>
        public float AbsentAmount { get; set; }

        /// <summary> 加班時數 </summary>
        public float OvertimeAmount { get; set; }

        /// <summary> 假別單位 </summary>
        public string AbsentUnit { get; set; }

        /// <summary> 請假理由 </summary>
        public string LeaveReason { get; set; }

        /// <summary> 建單日期 </summary>
        public DateTime CreateDate { get; set; }

        /// <summary> 假別名稱 </summary>
        public string AbsentName { get; set; }

        /// <summary>
        /// 假別英文名稱
        /// </summary>
        public string AbsentNameEN { get; set; }

        /// <summary> 公司代號 </summary>
        public string CompanyCode { get; set; }

        /// <summary> 公司名稱 </summary>
        public string CompanyName { get; set; }
    }
}
