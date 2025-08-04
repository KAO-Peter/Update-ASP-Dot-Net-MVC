namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaPerformanceMetaData))]
    public partial class PfaPerformance
    {
    }
    
    public partial class PfaPerformanceMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Code { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Name { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        public Nullable<int> Ordering { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Performance { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string band { get; set; }
        public Nullable<int> Rates { get; set; }
        public Nullable<decimal> Multiplier { get; set; }
        public Nullable<decimal> ScoresStart { get; set; }
        public Nullable<decimal> ScoresEnd { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual ICollection<PfaCycleRationDetail> PfaCycleRationDetail { get; set; }
        public virtual ICollection<PfaCycleEmp> PfaCycleEmp { get; set; }
        public virtual ICollection<PfaCycleEmp> PfaCycleEmp1 { get; set; }
    }
}
