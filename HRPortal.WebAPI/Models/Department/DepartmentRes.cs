using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRPortal.WebAPI
{
    /// <summary>
    /// 部門資訊
    /// </summary>
    public class DepartmentRes
    {
        /// <summary>
        /// 公司別
        /// </summary>
        public string CompanyID { get; set; }
        /// <summary>
        /// 分公司名稱
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 系統別
        /// </summary>
        public string SystemID { get; set; }
        /// <summary>
        /// 部門代碼
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// 部門名稱
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 上層部門代碼
        /// </summary>
        public string SignManagerCode { get; set; }
        /// <summary>
        /// 上層部門全名
        /// </summary>
        public string SignManagerName { get; set; }
        /// <summary>
        /// 主管工號
        /// </summary>
        public string ManagerEmployeeNO { get; set; }
        /// <summary>
        /// 主管姓名
        /// </summary>
        public string ManagerEmployeeName { get; set; }
        /// <summary>
        /// 覆核主管工號
        /// </summary>
        public string ReviewManagerEmployeeNO { get; set; }
        /// <summary>
        /// 覆核主管姓名
        /// </summary>
        public string ReviewManagerEmployeeName { get; set; }
        /// <summary>
        /// 預留欄位帳號_1
        /// </summary>
        public string FirstReservedEmployeeNO { get; set; }
        /// <summary>
        /// 預留欄位帳號姓名_1
        /// </summary>
        public string FirstReservedEmployeeName { get; set; }
        /// <summary>
        /// 預留欄位帳號_2
        /// </summary>
        public string SecondReservedEmployeeNO { get; set; }
        /// <summary>
        /// 預留欄位帳號姓名_2
        /// </summary>
        public string SecondReservedEmployeeName { get; set; }
        /// <summary>
        /// 可會辦單位(零用金專用)
        /// </summary>
        public string ForCash { get; set; }
        /// <summary>
        /// 部門層級
        /// </summary>
        public int? DepartmentLevel { get; set; }
        /// <summary>
        /// 是否自動轉檔(新增)
        /// </summary>
        public string Transfer { get; set; }
        /// <summary>
        /// 是否註記
        /// </summary>
        public string Mark { get; set; }
    }

    
}