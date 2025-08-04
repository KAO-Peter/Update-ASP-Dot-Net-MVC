namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaRoleMetaData))]
    public partial class PfaRole
    {
    }
    
    public partial class PfaRoleMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(32, ErrorMessage="欄位長度不得大於 32 個字元")]
        [Required]
        public string Name { get; set; }
        public string RoleParams { get; set; }
        
        [StringLength(512, ErrorMessage="欄位長度不得大於 512 個字元")]
        public string Description { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual ICollection<PfaEmployee> Employees { get; set; }
        public virtual ICollection<PfaRoleMenuMaps> RoleMenuMaps { get; set; }
    }
}
