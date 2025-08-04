namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHRTimeOffTypeMetaData))]
    public partial class BambooHRTimeOffType
    {
    }
    
    public partial class BambooHRTimeOffTypeMetaData
    {
        [Required]
        public int ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string BambooHR_TimeOffTypeID { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Required]
        public string BambooHR_TimeOffTypeName { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string BambooHR_TimeOffTypeUnit { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string HRM_AbsentCode { get; set; }
        [Required]
        public int HRM_AbsentID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string HRM_AbsentName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string HRM_AbsentNameEN { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string HRM_AbsentUnit { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
    }
}
