namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaCycleRationDetailMetaData))]
    public partial class PfaCycleRationDetail
    {
    }
    
    public partial class PfaCycleRationDetailMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleRationID { get; set; }
        [Required]
        public System.Guid PfaPerformanceID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Code { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Name { get; set; }
        public Nullable<int> Ordering { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Performance { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string band { get; set; }
        public Nullable<int> Rates { get; set; }
        public Nullable<decimal> Multiplier { get; set; }
        public Nullable<decimal> ScoresStart { get; set; }
        public Nullable<decimal> ScoresEnd { get; set; }
        public Nullable<decimal> BudgetTotal { get; set; }
        public Nullable<int> Staffing { get; set; }
        public Nullable<decimal> TotalScore { get; set; }
        public Nullable<int> FirstFinal { get; set; }
        public Nullable<int> SecondFinal { get; set; }
        public Nullable<decimal> FinalRation { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<int> ThirdFinal { get; set; }
    
        public virtual PfaCycleRation PfaCycleRation { get; set; }
        public virtual PfaPerformance PfaPerformance { get; set; }
    }
}
