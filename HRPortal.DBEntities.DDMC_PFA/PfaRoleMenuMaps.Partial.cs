namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaRoleMenuMapsMetaData))]
    public partial class PfaRoleMenuMaps
    {
    }
    
    public partial class PfaRoleMenuMapsMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid Role_ID { get; set; }
        [Required]
        public System.Guid Menu_ID { get; set; }
        public string MenuParams { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual PfaMenu Menus { get; set; }
        public virtual PfaRole Roles { get; set; }
    }
}
