namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaDepartmentMetaData))]
    public partial class PfaDepartment
    {
    }
    
    public partial class PfaDepartmentMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(12, ErrorMessage="欄位長度不得大於 12 個字元")]
        [Required]
        public string DepartmentCode { get; set; }
        
        [StringLength(64, ErrorMessage="欄位長度不得大於 64 個字元")]
        [Required]
        public string DepartmentName { get; set; }
        
        [StringLength(64, ErrorMessage="欄位長度不得大於 64 個字元")]
        public string DepartmentEnglishName { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        public Nullable<System.Guid> ParentDepartmentID { get; set; }
        public Nullable<System.Guid> SignParentID { get; set; }
        public Nullable<System.Guid> ManagerId { get; set; }
        public Nullable<System.Guid> SignManagerId { get; set; }
        [Required]
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        [Required]
        public bool OnlyForSign { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<int> Department_ID { get; set; }
        public Nullable<int> DepartmentsLevel { get; set; }
    
        public virtual ICollection<PfaEmployee> Employees { get; set; }
        public virtual PfaEmployee Employees1 { get; set; }
        public virtual ICollection<PfaDepartment> Departments1 { get; set; }
        public virtual PfaDepartment Departments2 { get; set; }
        public virtual PfaEmployee Employees2 { get; set; }
        public virtual ICollection<PfaDepartment> Departments11 { get; set; }
        public virtual PfaDepartment Departments3 { get; set; }
        public virtual ICollection<PfaEmployee> Employees3 { get; set; }
        public virtual PfaCompany Companys { get; set; }
    }
}
