namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaCycleMetaData))]
    public partial class PfaCycle
    {
    }
    
    public partial class PfaCycleMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaFormNo { get; set; }
        
        [StringLength(4, ErrorMessage="欄位長度不得大於 4 個字元")]
        public string PfaYear { get; set; }
        public Nullable<System.DateTime> ServeBasedate { get; set; }
        public Nullable<System.DateTime> DutyBeginDate { get; set; }
        public Nullable<System.DateTime> DutyEndDate { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string Desription { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string Status { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<System.DateTime> AssumeBasedate { get; set; }
    
        public virtual PfaCompany Companys { get; set; }
        public virtual ICollection<PfaCycleRation> PfaCycleRation { get; set; }
        public virtual ICollection<PfaMailMessage> PfaMailMessage { get; set; }
        public virtual ICollection<PfaSignerList> PfaSignerList { get; set; }
    }
}
