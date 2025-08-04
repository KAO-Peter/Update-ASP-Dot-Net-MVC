namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmpAbilityDetailMetaData))]
    public partial class PfaEmpAbilityDetail
    {
    }
    
    public partial class PfaEmpAbilityDetailMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaEmpAbilityID { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        public string PfaAbilityKey { get; set; }
        public Nullable<int> UpperLimit { get; set; }
        public Nullable<int> LowerLimit { get; set; }
        public Nullable<int> Ordering { get; set; }
        public Nullable<double> AbilityScore { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual PfaEmpAbility PfaEmpAbility { get; set; }
    }
}
