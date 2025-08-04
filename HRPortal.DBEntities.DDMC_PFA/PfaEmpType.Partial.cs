namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmpTypeMetaData))]
    public partial class PfaEmpType
    {
    }
    
    public partial class PfaEmpTypeMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaEmpTypeCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaEmpTypeName { get; set; }
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
        public virtual ICollection<PfaEmpTypeTargets> PfaEmpTypeTargets { get; set; }
        public virtual ICollection<PfaAbility> PfaAbility { get; set; }
        public virtual ICollection<PfaSignProcess> PfaSignProcess { get; set; }
        public virtual ICollection<PfaSignRecord> PfaSignRecord { get; set; }
        public virtual ICollection<PfaCycleEmp> PfaCycleEmp { get; set; }
    }
}
