namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmpIndicatorMetaData))]
    public partial class PfaEmpIndicator
    {
    }
    
    public partial class PfaEmpIndicatorMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleEmpID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string PfaIndicatorCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string PfaIndicatorName { get; set; }
        public string Description { get; set; }
        public Nullable<int> Scale { get; set; }
        public Nullable<int> Ordering { get; set; }
        public string PfaIndicatorDesc { get; set; }
        public Nullable<double> SelfIndicator { get; set; }
        public Nullable<double> ManagerIndicator { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual PfaCycleEmp PfaCycleEmp { get; set; }
    }
}
