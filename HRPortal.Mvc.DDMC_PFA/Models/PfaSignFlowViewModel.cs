using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaSignFlowViewModel
    {
        public System.Guid ID { get; set; }
        public CreatePfaSignFlowView[] Data { get; set; }
        public List<SelectListItem> CompanyList { get; set; }
        public List<SelectListItem> DepartmentList { get; set; }
        public List<SelectListItem> SignTypeList { get; set; }
        public List<SelectListItem> SignLevelList { get; set; }
        public List<SelectListItem> DeptClassList { get; set; }
    }

    public class CreatePfaSignFlowView
    {
        /// <summary>
        /// 簽核STEP
        /// </summary>
        public int SignStep { get; set; }
        /// <summary>
        /// 簽核關卡
        /// </summary>
        public System.Guid SignLevelID { get; set; }
        public string SignLevelCode { get; set; }
        /// <summary>
        /// 部門階級ID
        /// </summary>
        public System.Guid? DeptClassID { get; set; }
        /// <summary>
        /// 特定員工ID
        /// </summary>
        public System.Guid? EmployeesID { get; set; }
        public string EmployeesNo { get; set; }
        public string EmployeesName { get; set; }
        public System.Guid? CompanysID { get; set; }
        /// <summary>
        /// 特定部門ID
        /// </summary>
        public System.Guid? DepartmentsID { get; set; }
        /// <summary>
        /// 是否自評
        /// </summary>
        public bool IsSelfEvaluation { get; set; }
        /// <summary>
        /// 是否初核
        /// </summary>
        public bool IsFirstEvaluation { get; set; }
        /// <summary>
        /// 是否複核
        /// </summary>
        public bool IsSecondEvaluation { get; set; }
        /// <summary>
        /// 是否核決
        /// </summary>
        public bool IsThirdEvaluation { get; set; }
        /// <summary>
        /// 上傳權限
        /// </summary>
        public bool IsUpload { get; set; }

        public List<SelectListItem> CompanyList { get; set; }
        public List<SelectListItem> DepartmentList { get; set; }
        public List<SelectListItem> SignTypeList { get; set; }
        public List<SelectListItem> SignLevelList { get; set; }
        public List<SelectListItem> DeptClassList { get; set; }
    }

    public class SelectPfaTargets
    {
        [Required(ErrorMessage = "請輸入身分類別ID")]
        public Guid SignTypeID { get; set; }

        public List<JobTitleItem> JobTitleItems { get; set; }
    }
}