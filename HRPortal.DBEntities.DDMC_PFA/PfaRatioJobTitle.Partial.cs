namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaRatioJobTitleMetaData))]
    public partial class PfaRatioJobTitle
    {
    }
    
    public partial class PfaRatioJobTitleMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid JobTitleID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual PfaOption PfaOption { get; set; }
    }
}
