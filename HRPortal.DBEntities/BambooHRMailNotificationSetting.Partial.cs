namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHRMailNotificationSettingMetaData))]
    public partial class BambooHRMailNotificationSetting
    {
    }
    
    public partial class BambooHRMailNotificationSettingMetaData
    {
        [Required]
        public int ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Category { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public int RcptType { get; set; }
        [Required]
        public int CCType { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string CustomRcpt { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string CustomCC { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        public string DefaultSubject { get; set; }
        
        [StringLength(2000, ErrorMessage="欄位長度不得大於 2000 個字元")]
        public string DefaultContent { get; set; }
        [Required]
        public bool IsHTML { get; set; }
    }
}
