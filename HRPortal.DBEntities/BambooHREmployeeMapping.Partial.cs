namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHREmployeeMappingMetaData))]
    public partial class BambooHREmployeeMapping
    {
    }
    
    public partial class BambooHREmployeeMappingMetaData
    {
        [Required]
        public int ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string BambooHREmployeeID { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        [Required]
        public string BambooHREmail { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string BambooHREmployeeName { get; set; }
        public Nullable<System.Guid> PortalEmployeeID { get; set; }
        public Nullable<int> EmpData_ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string EmpID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string EmpName { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string DefaultAgentEmpID { get; set; }
        public Nullable<System.Guid> DefaultAgentPortalEmployeeID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string BambooHRUserID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string BambooHRUserFirstName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string BambooHRUserLastName { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string BambooHRUserEmail { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string BambooHRUserStatus { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string BambooHRUserLastLogin { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}
