namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(BambooHRLeaveFormRecordMetaData))]
    public partial class BambooHRLeaveFormRecord
    {
    }
    
    public partial class BambooHRLeaveFormRecordMetaData
    {
        [Required]
        public int ID { get; set; }
        public Nullable<System.Guid> PortalLeaveFormID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string BambooHRTimeOffID { get; set; }
        public Nullable<int> EmpData_ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string EmpID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string BambooHREmployeeID { get; set; }
        public Nullable<System.Guid> PortalEmployeeID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string FormNo { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string BambooHRStatus { get; set; }
        [Required]
        public int FormStatus { get; set; }
        
        [StringLength(10, ErrorMessage="欄位長度不得大於 10 個字元")]
        public string PortalAbsentCode { get; set; }
        public Nullable<System.DateTime> PortalStartTime { get; set; }
        public Nullable<System.DateTime> PortalEndTime { get; set; }
        public Nullable<decimal> PortalLeaveAmount { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        public string PortalAbsentUnit { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string PortalLeaveReason { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string AgentEmpID { get; set; }
        public Nullable<System.DateTime> BambooHRStartDate { get; set; }
        public Nullable<System.DateTime> BambooHREndDate { get; set; }
        public Nullable<decimal> BambooHRLeaveAmount { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string BambooHRLeaveReason { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string BambooHRManagerNote { get; set; }
        public Nullable<System.DateTime> BambooHRConfirmedStartTime { get; set; }
        public Nullable<System.DateTime> BambooHRConfirmedEndTime { get; set; }
        
        [StringLength(10, ErrorMessage="欄位長度不得大於 10 個字元")]
        public string CompanyCode { get; set; }
        public Nullable<bool> IsMailSent { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string Remark { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string UpdateFrom { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string CreateFrom { get; set; }
    
        public virtual LeaveForm LeaveForm { get; set; }
    }
}
