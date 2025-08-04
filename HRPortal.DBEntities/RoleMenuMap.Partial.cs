namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(RoleMenuMapMetaData))]
    public partial class RoleMenuMap
    {
    }
    
    public partial class RoleMenuMapMetaData
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
    
        public virtual Menu Menu { get; set; }
        public virtual Role Role { get; set; }
    }
}
