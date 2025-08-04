using System.Collections.Generic;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class SalaryChangeNoteInfo
    {
        /// <summary>
        /// 姓名
        ///</summary>
        public string Name { get; set; }
        /// <summary>
        /// 工號
        ///</summary>
        public string EmpID { get; set; }
        /// <summary>
        /// 異動原因
        ///</summary>
        public string ChangeReason { get; set; }
        /// <summary>
        /// 生效日
        ///</summary>
        public string EffectiveDate { get; set; }
        /// <summary>
        /// 執行日/發文日期
        ///</summary>
        public string ExecutionDate { get; set; }
        /// <summary>
        /// 生效日
        ///</summary>
        public string AppplyDate { get; set; }
        /// <summary>
        /// 異動單號
        ///</summary>
        public string FormNo { get; set; }

        #region 異動前
        /// <summary>
        /// 部門名稱
        ///</summary>
        public string BefDeptName { get; set; }
        /// <summary>
        /// 資位
        ///</summary>
        public string BefJobFunction { get; set; }
        /// <summary>
        /// 職稱
        ///</summary>
        public string BefJobTitle { get; set; }
        /// <summary>
        /// 職等
        ///</summary>
        public string BefGrade { get; set; }
        /// <summary>
        /// 薪點
        ///</summary>
        public string BefSalarybasis { get; set; }
        /// <summary>
        /// 合計
        ///</summary>
        public string BefTotalAmount { get; set; }
        #endregion

        #region 異動後
        /// 部門名稱
        ///</summary>
        public string AftDeptName { get; set; }
        /// <summary>
        /// 資位
        ///</summary>
        public string AftJobFunction { get; set; }
        /// <summary>
        /// 職稱
        ///</summary>
        public string AftJobTitle { get; set; }
        /// <summary>
        /// 職等
        ///</summary>
        public string AftGrade { get; set; }
        /// <summary>
        /// 薪點
        ///</summary>
        public string AftSalarybasis { get; set; }
        /// <summary>
        /// 合計
        ///</summary>
        public string AftTotalAmount { get; set; }

        #endregion

        /// <summary>
        /// 薪津
        /// </summary>
        public List<SalaryDataList> SalaryData { get; set; }

    }

    public class SalaryDataList
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Salary_b { get; set; }
        public string Salary_a { get; set; }
    }
        
}