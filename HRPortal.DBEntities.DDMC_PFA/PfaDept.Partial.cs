namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaDeptMetaData))]
    public partial class PfaDept
    {
    }
    
    public partial class PfaDeptMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaDeptCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaDeptName { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        public Nullable<System.Guid> ParentDepartmentID { get; set; }
        public Nullable<System.Guid> SignParentID { get; set; }
        public Nullable<System.Guid> ManagerID { get; set; }
        public Nullable<System.Guid> SignManagerID { get; set; }
        [Required]
        public System.Guid DeptClassID { get; set; }
        [Required]
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        [Required]
        public bool OnlyForSign { get; set; }
        public Nullable<int> Department_ID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual ICollection<PfaDeptEmp> PfaDeptEmp { get; set; }
    }
}
