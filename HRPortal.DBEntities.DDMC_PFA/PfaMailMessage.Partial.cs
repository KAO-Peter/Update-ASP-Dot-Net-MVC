namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaMailMessageMetaData))]
    public partial class PfaMailMessage
    {
    }
    
    public partial class PfaMailMessageMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleID { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string SourceType { get; set; }
        [Required]
        public System.Guid FromAccountID { get; set; }
        public string Rcpt { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string Cc { get; set; }
        
        [StringLength(500, ErrorMessage="欄位長度不得大於 500 個字元")]
        public string Bcc { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public bool IsHtml { get; set; }
        public Nullable<bool> IsCancel { get; set; }
        
        [StringLength(255, ErrorMessage="欄位長度不得大於 255 個字元")]
        public string Remark { get; set; }
        [Required]
        public bool HadSend { get; set; }
        public Nullable<System.DateTime> SendTimeStamp { get; set; }
        public Nullable<System.DateTime> ErrorTimeStamp { get; set; }
        public string ErrorMessage { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
        public Nullable<int> MailType { get; set; }
        public Nullable<int> SendStatus { get; set; }
        public Nullable<System.DateTime> LastUpdateTime { get; set; }
    
        public virtual PfaEmployee Employees { get; set; }
        public virtual PfaMailAccount MailAccount { get; set; }
        public virtual PfaCycle PfaCycle { get; set; }
    }
}
