namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSignProcessMetaData))]
    public partial class PfaSignProcess
    {
    }
    
    public partial class PfaSignProcessMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleEmpID { get; set; }
        [Required]
        public int SignStep { get; set; }
        [Required]
        public System.Guid SignLevelID { get; set; }
        [Required]
        public bool IsSelfEvaluation { get; set; }
        [Required]
        public bool IsFirstEvaluation { get; set; }
        [Required]
        public bool IsSecondEvaluation { get; set; }
        [Required]
        public bool IsUpload { get; set; }
        public Nullable<System.Guid> PfaEmpTypeID { get; set; }
        [Required]
        public bool IsAgent { get; set; }
        [Required]
        public bool IsRatio { get; set; }
        [Required]
        public System.Guid OrgSignEmpID { get; set; }
        [Required]
        public System.Guid PreSignEmpID { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string Status { get; set; }
        public Nullable<System.Guid> SignEmpID { get; set; }
        public Nullable<System.DateTime> ConfirmTime { get; set; }
        public Nullable<System.DateTime> SignTime { get; set; }
        
        [StringLength(120, ErrorMessage="欄位長度不得大於 120 個字元")]
        public string Assessment { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        [Required]
        public bool IsThirdEvaluation { get; set; }
    
        public virtual PfaEmployee Employees { get; set; }
        public virtual PfaEmployee Employees1 { get; set; }
        public virtual PfaEmployee Employees2 { get; set; }
        public virtual PfaEmpType PfaEmpType { get; set; }
        public virtual PfaOption PfaOption { get; set; }
        public virtual ICollection<PfaSignUpload> PfaSignUpload { get; set; }
        public virtual PfaCycleEmp PfaCycleEmp { get; set; }
    }
}
