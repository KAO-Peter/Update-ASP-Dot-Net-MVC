namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSystemLogMetaData))]
    public partial class PfaSystemLog
    {
    }
    
    public partial class PfaSystemLogMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.DateTime LogTime { get; set; }
        [Required]
        public System.Guid UserID { get; set; }
        
        [StringLength(15, ErrorMessage="欄位長度不得大於 15 個字元")]
        [Required]
        public string UserIP { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        public string Controller { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        public string Action { get; set; }
        public string Remark { get; set; }
    
        public virtual PfaEmployee Employee { get; set; }
    }
}
