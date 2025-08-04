namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaEmpTypeTargetsMetaData))]
    public partial class PfaEmpTypeTargets
    {
    }
    
    public partial class PfaEmpTypeTargetsMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaEmpTypeID { get; set; }
        [Required]
        public System.Guid JobTitleID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual PfaEmpType PfaEmpType { get; set; }
    }
}
