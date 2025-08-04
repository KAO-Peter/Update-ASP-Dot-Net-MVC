namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmpAbilityMetaData))]
    public partial class PfaEmpAbility
    {
    }
    
    public partial class PfaEmpAbilityMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleEmpID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaAbilityCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string PfaAbilityName { get; set; }
        public string Description { get; set; }
        public Nullable<int> TotalScore { get; set; }
        public Nullable<int> Ordering { get; set; }
        public Nullable<double> ManagerAbility { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual ICollection<PfaEmpAbilityDetail> PfaEmpAbilityDetail { get; set; }
        public virtual PfaCycleEmp PfaCycleEmp { get; set; }
    }
}
