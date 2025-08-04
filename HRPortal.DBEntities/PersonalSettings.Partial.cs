namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PersonalSettingsMetaData))]
    public partial class PersonalSettings
    {
    }
    
    public partial class PersonalSettingsMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Type { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string CompanyCode { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string EmployeeNO { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string SettingKey { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string SettingValue { get; set; }
        [Required]
        public System.DateTime ModifiedTime { get; set; }
    }
}
