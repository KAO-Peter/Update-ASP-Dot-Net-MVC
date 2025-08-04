namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(AnnouncementFileMetaData))]
    public partial class AnnouncementFile
    {
    }
    
    public partial class AnnouncementFileMetaData
    {
        [Required]
        public System.Guid AnnouncementID { get; set; }
        [Required]
        public System.Guid DownloadFilesID { get; set; }

        public virtual Announcement Announcement { get; set; }
        public virtual DownloadFile DownloadFile { get; set; }
    }
}
