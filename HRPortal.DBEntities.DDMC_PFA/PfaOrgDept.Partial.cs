namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaOrgDeptMetaData))]
    public partial class PfaOrgDept
    {
    }
    
    public partial class PfaOrgDeptMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaOrgID { get; set; }
        [Required]
        public System.Guid PfaDeptID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual PfaDept PfaDept { get; set; }
        public virtual PfaOrg PfaOrg { get; set; }
    }
}
