namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(DownloadFileMetaData))]
    public partial class DownloadFile
    {
    }
    
    public partial class DownloadFileMetaData
    {
        [Required]
        public System.Guid Id { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DownloadName")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        public string OriginalName { get; set; }

        //[Required]
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DownloadFilePathName")]
        public string Path { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DownloadDescription")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DownloadSize")]
        public Nullable<int> Size { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DownloadFormat")]
        public string Format { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Createdby")]
        public System.Guid Createdby { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "CreatedTime")]
        public System.DateTime CreatedTime { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "CreatedTime")]
        public Nullable<System.Guid> Modifiedby { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "ModifiedTime")]
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }

        /// <summary>
        /// Createdby foreign key
        /// </summary>
        public virtual Employee Employee { get; set; }

        /// <summary>
        /// Modifiedby foreign key
        /// </summary>
        public virtual Employee Employee1 { get; set; }

        /// <summary>
        /// Deletedby foreign key
        /// </summary>
        public virtual Employee Employee2 { get; set; }
        public virtual ICollection<AnnouncementFile> AnnouncementFiles { get; set; }
    }
}
