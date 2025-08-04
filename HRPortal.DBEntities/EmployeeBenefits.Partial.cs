namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(EmployeeBenefitsMetaData))]
    public partial class EmployeeBenefits
    {
    }
    
    public partial class EmployeeBenefitsMetaData
    {
        [Required]
        public System.Guid Id { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Title { get; set; }
        [Required]
        public string ContentText { get; set; }
        [Required]
        public System.DateTime AnnounceStartTime { get; set; }
        public Nullable<System.DateTime> AnnounceEndTime { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public System.Guid Createdby { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> Modifiedby { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }
        [Required]
        public bool IsSticky { get; set; }
    
        public virtual Employee Employees { get; set; }
        public virtual Employee Employees1 { get; set; }
        public virtual ICollection<EmployeeBenefitsFiles> EmployeeBenefitsFiles { get; set; }
    }
}
