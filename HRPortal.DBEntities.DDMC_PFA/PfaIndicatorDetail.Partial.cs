namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaIndicatorDetailMetaData))]
    public partial class PfaIndicatorDetail
    {
    }
    
    public partial class PfaIndicatorDetailMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaIndicatorID { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        public string PfaIndicatorKey { get; set; }
        public Nullable<decimal> UpperLimit { get; set; }
        public Nullable<decimal> LowerLimit { get; set; }
        public Nullable<int> Ordering { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual PfaIndicator PfaIndicator { get; set; }
    }
}
