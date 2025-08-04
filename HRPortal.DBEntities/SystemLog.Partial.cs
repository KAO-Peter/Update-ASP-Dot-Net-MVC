namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(SystemLogMetaData))]
    public partial class SystemLog
    {
    }
    
    public partial class SystemLogMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "QueryLogTime")]
        public System.DateTime LogTime { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "QueryLogOperator")]
        public System.Guid UserID { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "QueryController")]
        public string Controller { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "QueryAction")]
        public string Action { get; set; }
        
        //[StringLength(4000, ErrorMessage="欄位長度不得大於 4000 個字元")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "QueryRemark")]
        public string Remark { get; set; }

        /// <summary>
        /// UserID
        /// </summary>
        public virtual Employee Employee { get; set; }
    }
}
