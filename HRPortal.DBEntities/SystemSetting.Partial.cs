namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(SystemSettingMetaData))]
    public partial class SystemSetting
    {
    }
    
    public partial class SystemSettingMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        public string SettingKey { get; set; }
        
        [StringLength(150, ErrorMessage="欄位長度不得大於 150 個字元")]
        [Required]
        public string SettingValue { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
    }
}
