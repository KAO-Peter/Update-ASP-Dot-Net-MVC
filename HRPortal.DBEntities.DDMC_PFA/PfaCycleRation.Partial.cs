namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaCycleRationMetaData))]
    public partial class PfaCycleRation
    {
    }
    
    public partial class PfaCycleRationMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleID { get; set; }
        [Required]
        public System.Guid PfaOrgID { get; set; }
        public Nullable<int> OrgTotal { get; set; }
        public Nullable<decimal> TotalScore { get; set; }
        public Nullable<int> SelfFinal { get; set; }
        public Nullable<int> FirstFinal { get; set; }
        public Nullable<int> SecondFinal { get; set; }
        public Nullable<decimal> FinalRation { get; set; }
        [Required]
        public bool IsRation { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<int> ThirdFinal { get; set; }
    
        public virtual PfaOrg PfaOrg { get; set; }
        public virtual ICollection<PfaCycleRationDetail> PfaCycleRationDetail { get; set; }
        public virtual PfaCycle PfaCycle { get; set; }
    }
}
