namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(RoleMetaData))]
    public partial class Role
    {
    }
    
    public partial class RoleMetaData
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
    
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<RoleMenuMap> RoleMenuMaps { get; set; }
    }
}
