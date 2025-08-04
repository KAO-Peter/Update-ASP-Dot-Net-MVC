namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(EmployeeMetaData))]
    public partial class Employee
    {
        public System.Guid SignDepartmentID1 { get; set; }
        public System.DateTime EffectiveDate { get; set; }
    }

    public partial class EmployeeMetaData
    {
        [Required]
        public System.Guid ID { get; set; }

        [StringLength(16, ErrorMessage = "欄位長度不得大於 16 個字元")]
        [Required]
        [Display(Name = "工號")]
        public string EmployeeNO { get; set; }

        [StringLength(32, ErrorMessage = "欄位長度不得大於 32 個字元")]
        [Required]
        [Display(Name = "姓名")]
        public string EmployeeName { get; set; }

        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string EmployeeEnglishName { get; set; }
        [Required]
        [Display(Name = "公司別")]
        public System.Guid CompanyID { get; set; }
        [Required]
        [Display(Name="所屬部門")]
        public System.Guid DepartmentID { get; set; }
        [Required]
        [Display(Name = "簽核組織部門")]
        public System.Guid SignDepartmentID { get; set; }
        public Nullable<System.Guid> ReportToID { get; set; }
        [Required]
        [Display(Name = "性別")]
        public short Gender { get; set; }

        [StringLength(16, ErrorMessage = "欄位長度不得大於 16 個字元")]
        public string CellphoneNumber { get; set; }

        [StringLength(16, ErrorMessage = "欄位長度不得大於 16 個字元")]
        public string TelephoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "欄位長度不得大於 256 個字元")]
        public string Email { get; set; }
        [Required]
        [Display(Name="報到時間")]
        public System.DateTime ArriveDate { get; set; }
        public Nullable<System.DateTime> LeaveDate { get; set; }
        public Nullable<System.DateTime> DisableDate { get; set; }
        [Required]
        public bool Enabled { get; set; }
        
        [Required]
        public System.DateTime CreatedTime { get; set; }

        [StringLength(256, ErrorMessage = "欄位長度不得大於 256 個字元")]
        public string PasswordHash { get; set; }
        public Nullable<System.DateTime> PasswordExpiredDate { get; set; }
        [Required]
        [Display(Name="權限")]
        public System.Guid RoleID { get; set; }
        [Required]
        public bool TopExecutive { get; set; }

        public virtual Company Company { get; set; }
        public virtual Department Department { get; set; }
        public virtual Department SignDepartment { get; set; }
        public virtual ICollection<Department> ManageDepartments { get; set; }
        public virtual ICollection<Employee> ReportFrom { get; set; }
        public virtual Employee ReportTo { get; set; }
        public virtual Role Role { get; set; }
    }
}