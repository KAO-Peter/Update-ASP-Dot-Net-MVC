namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHRHistoryLeaveFormMetaData))]
    public partial class BambooHRHistoryLeaveForm
    {
    }
    
    public partial class BambooHRHistoryLeaveFormMetaData
    {
        [Required]
        public int ID { get; set; }
        [Required]
        public int EmpData_ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string EmpID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string FormNo { get; set; }
        public Nullable<System.Guid> PortalLeaveFormID { get; set; }
        public Nullable<int> EmpAbsent_ID { get; set; }
        public Nullable<int> EmpWorkAdjust_ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string AbsentCode { get; set; }
        [Required]
        public System.DateTime BeginTime { get; set; }
        [Required]
        public System.DateTime EndTime { get; set; }
        [Required]
        public decimal AbsentAmount { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string AbsentUnit { get; set; }
        [Required]
        public int FormStatus { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
    }
}
