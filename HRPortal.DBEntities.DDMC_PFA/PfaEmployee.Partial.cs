namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmployeeMetaData))]
    public partial class PfaEmployee
    {
    }
    
    public partial class PfaEmployeeMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string EmployeeNO { get; set; }
        
        [StringLength(32, ErrorMessage="欄位長度不得大於 32 個字元")]
        [Required]
        public string EmployeeName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string EmployeeEnglishName { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid DepartmentID { get; set; }
        [Required]
        public System.Guid SignDepartmentID { get; set; }
        public Nullable<System.Guid> ReportToID { get; set; }
        [Required]
        public short Gender { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        public string CellphoneNumber { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        public string TelephoneNumber { get; set; }
        
        [StringLength(256, ErrorMessage="欄位長度不得大於 256 個字元")]
        public string Email { get; set; }
        public Nullable<int> Employee_ID { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string EmployeeType { get; set; }
        
        [StringLength(150, ErrorMessage="欄位長度不得大於 150 個字元")]
        public string Address { get; set; }
        
        [StringLength(150, ErrorMessage="欄位長度不得大於 150 個字元")]
        public string RegisterAddress { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        public string RegisterTelephoneNumber { get; set; }
        
        [StringLength(32, ErrorMessage="欄位長度不得大於 32 個字元")]
        public string EmergencyName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string EmergencyRelation { get; set; }
        
        [StringLength(150, ErrorMessage="欄位長度不得大於 150 個字元")]
        public string EmergencyAddress { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        public string EmergencyTelephoneNumber { get; set; }
        [Required]
        public System.DateTime ArriveDate { get; set; }
        public Nullable<System.DateTime> LeaveDate { get; set; }
        public Nullable<System.DateTime> DisableDate { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        
        [StringLength(256, ErrorMessage="欄位長度不得大於 256 個字元")]
        public string PasswordHash { get; set; }
        public Nullable<System.DateTime> PasswordExpiredDate { get; set; }
        [Required]
        public System.Guid RoleID { get; set; }
        [Required]
        public bool TopExecutive { get; set; }
        public Nullable<int> EmployeesLevel { get; set; }
        [Required]
        public int PasswordFailedCount { get; set; }
        [Required]
        public bool PasswordLockStatus { get; set; }
        public Nullable<System.DateTime> PasswordLockDate { get; set; }
        
        [StringLength(256, ErrorMessage="欄位長度不得大於 256 個字元")]
        public string SalaryPasswordHash { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        public string FirstModifiedFlag { get; set; }
        
        [StringLength(256, ErrorMessage="欄位長度不得大於 256 個字元")]
        public string PersonalEmail { get; set; }
        
        [StringLength(256, ErrorMessage="欄位長度不得大於 256 個字元")]
        public string DesignatedPerson { get; set; }
    
        public virtual PfaDepartment Department { get; set; }
        public virtual ICollection<PfaDepartment> Departments1 { get; set; }
        public virtual ICollection<PfaDepartment> Departments2 { get; set; }
        public virtual PfaDepartment SignDepartment { get; set; }
        public virtual ICollection<PfaEmployee> Employees1 { get; set; }
        public virtual PfaEmployee Employees2 { get; set; }
        public virtual ICollection<PfaDeptEmp> PfaDeptEmp { get; set; }
        public virtual PfaRole Role { get; set; }
        public virtual PfaCompany Company { get; set; }
        public virtual ICollection<PfaSystemLog> SystemLog { get; set; }
        public virtual ICollection<PfaOrg> PfaOrg { get; set; }
        public virtual ICollection<PfaSignFlow> PfaSignFlow { get; set; }
        public virtual ICollection<PfaSignProcess> PfaSignProcess { get; set; }
        public virtual ICollection<PfaSignProcess> PfaSignProcess1 { get; set; }
        public virtual ICollection<PfaSignProcess> PfaSignProcess2 { get; set; }
        public virtual ICollection<PfaSignRecord> PfaSignRecord { get; set; }
        public virtual ICollection<PfaSignRecord> PfaSignRecord1 { get; set; }
        public virtual ICollection<PfaSignRecord> PfaSignRecord2 { get; set; }
        public virtual ICollection<PfaSignerList> PfaSignerList { get; set; }
        public virtual ICollection<PfaMailMessage> PfaMailMessage { get; set; }
        public virtual ICollection<PfaCycleEmp> PfaCycleEmp { get; set; }
    }
}
