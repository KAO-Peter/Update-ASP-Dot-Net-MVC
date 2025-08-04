namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(AnnouncementMetaData))]
    public partial class Announcement
    {
    }
    
    public partial class AnnouncementMetaData
    {
     
        public System.Guid Id { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementTitle")]
        [Required]
        public string Title { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementContentext")]
        public string ContentText { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementStartTime")]
        public System.DateTime AnnounceStartTime { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementEndTime")]
        public Nullable<System.DateTime> AnnounceEndTime { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementStatus")]
        public bool Status { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementSticky")]
        public bool IsSticky { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementCreatedBy")]
        public System.Guid Createdby { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementCreatedTime")]
        public System.DateTime CreatedTime { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementModifiedBy")]
        public Nullable<System.Guid> Modifiedby { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "AnnouncementModifiedTime")]
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        /// <summary>
        /// CreateBy
        /// </summary>
        public virtual Employee Employee { get; set; }
        /// <summary>
        /// ModifiedBy
        /// </summary>
        public virtual Employee Employee1 { get; set; }

        public virtual ICollection<AnnouncementFile> AnnouncementFiles { get; set; }

        [Required]
        public bool IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }

    }
}
