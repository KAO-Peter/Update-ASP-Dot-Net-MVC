namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(OptionMetaData))]
    public partial class Option
    {
    }
    
    public partial class OptionMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid OptionGroupID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string OptionValue { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string DisplayResourceName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string DisplayName { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual Employee Creator { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual OptionGroup OptionGroup { get; set; }
    }
}
