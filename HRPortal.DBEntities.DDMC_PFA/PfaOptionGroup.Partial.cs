namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaOptionGroupMetaData))]
    public partial class PfaOptionGroup
    {
    }
    
    public partial class PfaOptionGroupMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string OptionGroupCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string OptionGroupName { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        [Required]
        public int Ordering { get; set; }
        [Required]
        public System.Guid Createdby { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual ICollection<PfaOption> PfaOption { get; set; }
    }
}
