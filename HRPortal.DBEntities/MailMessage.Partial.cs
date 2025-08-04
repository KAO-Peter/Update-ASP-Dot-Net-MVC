namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(MailMessageMetaData))]
    public partial class MailMessage
    {
    }
    
    public partial class MailMessageMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid FromAccountID { get; set; }
        
        [Required]
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
        [Required]
        public bool HadSend { get; set; }
        public Nullable<System.DateTime> SendTimeStamp { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
        public Nullable<System.DateTime> ErrorTimeStamp { get; set; }
        public string ErrorMessage { get; set; }

        public virtual MailAccount MailAccount { get; set; }
    }
}
