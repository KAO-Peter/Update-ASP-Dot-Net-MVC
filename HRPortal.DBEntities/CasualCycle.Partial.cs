namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(CasualCycleMetaData))]
    public partial class CasualCycle
    {
    }
    
    public partial class CasualCycleMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public int CasualCycle_ID { get; set; }
        
        [StringLength(15, ErrorMessage="欄位長度不得大於 15 個字元")]
        [Required]
        public string CasualFormNo { get; set; }
        [Required]
        public System.DateTime CasualPayDate { get; set; }
        [Required]
        public System.DateTime CountBeginDate { get; set; }
        [Required]
        public System.DateTime CountEndDate { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string LockFlag { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
    
        public virtual Company Company { get; set; }
    }
}
