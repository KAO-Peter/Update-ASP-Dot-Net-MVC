namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHRLeaveFormRecordLogMetaData))]
    public partial class BambooHRLeaveFormRecordLog
    {
    }
    
    public partial class BambooHRLeaveFormRecordLogMetaData
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public int BambooHRLeaveFormRecord_ID { get; set; }
        public Nullable<System.Guid> UserID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string UserIP { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Controller { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string Action { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string LogSource { get; set; }
        
        [StringLength(2000, ErrorMessage="欄位長度不得大於 2000 個字元")]
        public string LogText { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
    }
}
