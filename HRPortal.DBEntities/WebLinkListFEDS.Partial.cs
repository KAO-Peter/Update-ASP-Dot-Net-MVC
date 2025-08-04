namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(WebLinkListFEDSMetaData))]
    public partial class WebLinkListFEDS
    {
    }
    
    public partial class WebLinkListFEDSMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        public Nullable<int> Number { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string WebName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string WebLink { get; set; }
        [Required]
        public System.Guid Createdby { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> Modifiedby { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }
        public Nullable<System.Guid> OptionID { get; set; }
    
        public virtual Option Options { get; set; }
    }
}
