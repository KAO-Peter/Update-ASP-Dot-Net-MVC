namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSystemSettingMetaData))]
    public partial class PfaSystemSetting
    {
    }
    
    public partial class PfaSystemSettingMetaData
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
