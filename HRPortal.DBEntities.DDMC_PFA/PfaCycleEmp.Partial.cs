namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaCycleEmpMetaData))]
    public partial class PfaCycleEmp
    {
    }
    
    public partial class PfaCycleEmpMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleID { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        [Required]
        public System.Guid PfaDeptID { get; set; }
        public Nullable<System.Guid> PfaOrgID { get; set; }
        public Nullable<System.Guid> HireID { get; set; }
        public Nullable<System.Guid> JobTitleID { get; set; }
        public Nullable<System.Guid> JobFunctionID { get; set; }
        public Nullable<System.Guid> GradeID { get; set; }
        public Nullable<System.Guid> PositionID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Education { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string SchoolName { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string DeptDescription { get; set; }
        [Required]
        public bool Degree { get; set; }
        public Nullable<double> PersonalLeave { get; set; }
        public Nullable<double> SickLeave { get; set; }
        public Nullable<double> LateLE { get; set; }
        public Nullable<double> AWL { get; set; }
        public Nullable<double> Salary01 { get; set; }
        public Nullable<double> Salary02 { get; set; }
        public Nullable<double> Salary03 { get; set; }
        public Nullable<double> Salary04 { get; set; }
        public Nullable<double> Salary05 { get; set; }
        public Nullable<double> Salary06 { get; set; }
        public Nullable<double> FullSalary { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Performance1 { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Performance2 { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Performance3 { get; set; }
        public Nullable<System.Guid> SignTypeID { get; set; }
        public Nullable<System.Guid> PfaEmpTypeID { get; set; }
        [Required]
        public bool IsAgent { get; set; }
        [Required]
        public bool IsRatio { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string Status { get; set; }
        public Nullable<double> SelfIndicator { get; set; }
        public Nullable<double> ManagerIndicator { get; set; }
        public Nullable<double> ManagerAbility { get; set; }
        public Nullable<double> PfaSelfScore { get; set; }
        public Nullable<double> PfaFirstScore { get; set; }
        public Nullable<System.Guid> FirstPerformance_ID { get; set; }
        public Nullable<double> PfaLastScore { get; set; }
        public Nullable<System.Guid> LastPerformance_ID { get; set; }
        public Nullable<double> PfaFinalScore { get; set; }
        public Nullable<System.Guid> FinalPerformance_ID { get; set; }
        public string SelfAppraisal { get; set; }
        public string FirstAppraisal { get; set; }
        public Nullable<System.Guid> PastPerformance { get; set; }
        public Nullable<System.Guid> NowPerformance { get; set; }
        public Nullable<System.Guid> Development { get; set; }
        public string LastAppraisal { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string FinalAppraisal { get; set; }
    
        public virtual PfaOption PfaOption { get; set; }
        public virtual PfaDept PfaDept { get; set; }
        public virtual PfaEmpType PfaEmpType { get; set; }
        public virtual PfaOption PfaOption1 { get; set; }
        public virtual PfaOption PfaOption2 { get; set; }
        public virtual PfaOption PfaOption3 { get; set; }
        public virtual PfaOption PfaOption4 { get; set; }
        public virtual PfaOption PfaOption5 { get; set; }
        public virtual PfaOrg PfaOrg { get; set; }
        public virtual PfaPerformance PfaPerformance { get; set; }
        public virtual PfaPerformance PfaPerformance1 { get; set; }
        public virtual ICollection<PfaEmpAbility> PfaEmpAbility { get; set; }
        public virtual ICollection<PfaEmpIndicator> PfaEmpIndicator { get; set; }
        public virtual ICollection<PfaEmpTraining> PfaEmpTraining { get; set; }
        public virtual ICollection<PfaSignProcess> PfaSignProcess { get; set; }
        public virtual ICollection<PfaSignRecord> PfaSignRecord { get; set; }
        public virtual ICollection<PfaSignUpload> PfaSignUpload { get; set; }
        public virtual PfaEmployee Employees { get; set; }
    }
}
