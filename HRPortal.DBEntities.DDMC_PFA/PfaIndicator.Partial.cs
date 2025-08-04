namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaIndicatorMetaData))]
    public partial class PfaIndicator
    {
    }
    
    public partial class PfaIndicatorMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaIndicatorCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string PfaIndicatorName { get; set; }
        public string Description { get; set; }
        public Nullable<int> Scale { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        public Nullable<int> Ordering { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual PfaCompany Companys { get; set; }
        public virtual ICollection<PfaIndicatorDetail> PfaIndicatorDetail { get; set; }
    }
}
