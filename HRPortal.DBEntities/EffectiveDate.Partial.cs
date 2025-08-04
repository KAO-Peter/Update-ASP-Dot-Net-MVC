namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(EffectiveDateMetaData))]
    public partial class EffectiveDate
    {
    }
    
    public partial class EffectiveDateMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public int Function_Type { get; set; }
        public Nullable<System.Guid> Function_ID { get; set; }
        public Nullable<System.Guid> CompanyID { get; set; }
        public Nullable<System.Guid> Employee_ID { get; set; }
        public Nullable<System.Guid> Change_ID { get; set; }
        public Nullable<System.DateTime> Change_Date { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        public string Change_Type { get; set; }
        public Nullable<System.DateTime> CreatedTime { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string CreatedUser { get; set; }
    }
}
