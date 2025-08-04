using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaDeptViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public System.Guid ID { get; set; }
        /// <summary>
        /// 公司ID
        /// </summary>
        public System.Guid CompanyID { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 部門代碼
        /// </summary>
        public string PfaDeptCode { get; set; }

        /// <summary>
        /// 部門名稱
        /// </summary>
        public string PfaDeptName { get; set; }

        /// <summary>
        /// 管理者ID
        /// </summary>
        public System.Guid? ManagerID { get; set; }

        /// <summary>
        /// 管理者名稱
        /// </summary>
        public string ManagerName { get; set; }

        /// <summary>
        /// 管理者編號
        /// </summary>
        public string ManagerNo { get; set; }

        /// <summary>
        /// 上層部門ID
        /// </summary>
        public System.Guid? ParentDepartmentID { get; set; }

        /// <summary>
        /// 上層部門名稱
        /// </summary>
        public string ParentDepartmentName { get; set; }

        /// <summary>
        /// 上層簽核部門ID
        /// </summary>
        public System.Guid? SignParentID { get; set; }

        /// <summary>
        /// 上層簽核部門名稱
        /// </summary>
        public string SignParentName { get; set; }

        /// <summary>
        /// 簽核管理者ID
        /// </summary>
        public System.Guid? SignManagerID { get; set; }

        /// <summary>
        /// 簽核管理者名稱
        /// </summary>
        public string SignManagerName { get; set; }

        /// <summary>
        /// 簽核管理者編號
        /// </summary>
        public string SignManagerNo { get; set; }

        /// <summary>
        /// 部門階級ID
        /// </summary>
        public System.Guid DeptClassID { get; set; }

        /// <summary>
        /// 部門階級名稱
        /// </summary>
        public string DeptClassName { get; set; }

        /// <summary>
        /// 是否只用在簽核
        /// </summary>
        public bool OnlyForSign { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SelEmp> SelEmps { get; set; }
    }

    public class CreatePfaDept
    {
        [Required(ErrorMessage = "請輸入考核部門ID")]
        public System.Guid ID { get; set; }
        [Required(ErrorMessage = "請輸入公司別")]
        public System.Guid CompanyID { get; set; }
        [Required(ErrorMessage = "請輸入考核部門代碼")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaDeptCode { get; set; }
        [Required(ErrorMessage = "請輸入考核部門名稱")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaDeptName { get; set; }
        public System.Guid? ParentDepartmentID { get; set; }
        public System.Guid? SignParentID { get; set; }
        public System.Guid? ManagerID { get; set; }
        public System.Guid? SignManagerID { get; set; }
        public string SignManagerName { get; set; }
        [Required(ErrorMessage = "請輸入部門階級")]
        public System.Guid DeptClassID { get; set; }
        [Required(ErrorMessage = "請輸入部門類別")]
        public System.Guid DeptTypeID { get; set; }
        [Required(ErrorMessage = "請輸入有效起日")]
        public System.DateTime BeginDate { get; set; }
        public System.DateTime? EndDate { get; set; }

        public List<SelectListItem> CompanyList { get; set; }
        public List<SelectListItem> SignParentList { get; set; }
        public List<SelectListItem> DeptClassList { get; set; }
    }

    public class SelEmp
    {
        /// <summary>
        /// ID
        /// </summary>
        public System.Guid EmployeeID { get; set; }

        /// <summary>
        /// 員工編號
        /// </summary>
        public string EmployeeNO { get; set; }

        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// 離職日
        /// </summary>
        public DateTime? LeaveDate { get; set; }
    }
}