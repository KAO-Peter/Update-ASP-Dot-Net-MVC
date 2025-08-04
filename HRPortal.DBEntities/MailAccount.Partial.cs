namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(MailAccountMetaData))]
    public partial class MailAccount
    {
    }
    
    public partial class MailAccountMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string MailAddress { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string SmtpServer { get; set; }
        [Required]
        public int SmtpServerPort { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string UserName { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string UserPassword { get; set; }
        [Required]
        public bool SslEnabled { get; set; }
        [Required]
        public System.DateTime CreateTime { get; set; }
    
        public virtual ICollection<MailMessage> MailMessages { get; set; }
    }
}
