namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(MenuMetaData))]
    public partial class Menu
    {
    }
    
    public partial class MenuMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(128, ErrorMessage="欄位長度不得大於 128 個字元")]
        public string Title { get; set; }
        
        [StringLength(32, ErrorMessage="欄位長度不得大於 32 個字元")]
        public string Alias { get; set; }
        
        [StringLength(128, ErrorMessage="欄位長度不得大於 128 個字元")]
        public string Link { get; set; }
        [Required]
        public int Type { get; set; }
        [Required]
        public int Ordering { get; set; }
        public Nullable<System.Guid> Parent_ID { get; set; }
        
        [StringLength(512, ErrorMessage="欄位長度不得大於 512 個字元")]
        public string Description { get; set; }
        public Nullable<System.DateTime> DisableDate { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    
        public virtual ICollection<Menu> Menus1 { get; set; }
        public virtual Menu Menu1 { get; set; }
        public virtual ICollection<RoleMenuMap> RoleMenuMaps { get; set; }
    }
}
