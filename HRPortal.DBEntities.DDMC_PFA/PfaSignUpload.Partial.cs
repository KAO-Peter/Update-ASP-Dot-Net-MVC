namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSignUploadMetaData))]
    public partial class PfaSignUpload
    {
    }
    
    public partial class PfaSignUploadMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleEmpID { get; set; }
        [Required]
        public System.Guid PfaSignProcessID { get; set; }
        [Required]
        public int Ordering { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Required]
        public string Href { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string FileName { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual PfaSignProcess PfaSignProcess { get; set; }
        public virtual PfaCycleEmp PfaCycleEmp { get; set; }
    }
}
