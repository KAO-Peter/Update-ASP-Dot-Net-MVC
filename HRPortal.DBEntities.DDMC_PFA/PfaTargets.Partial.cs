namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaTargetsMetaData))]
    public partial class PfaTargets
    {
    }
    
    public partial class PfaTargetsMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid SignTypeID { get; set; }
        [Required]
        public System.Guid JobTitleID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    }
}
