namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(OptionGroupMetaData))]
    public partial class OptionGroup
    {
    }
    
    public partial class OptionGroupMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string OptionGroupKey { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string OptionGroupName { get; set; }
        public Nullable<System.Guid> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual Employee Creator { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual ICollection<Option> Options { get; set; }
    }
}
