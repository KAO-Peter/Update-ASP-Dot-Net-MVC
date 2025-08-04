namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(ReadTodayFilesMetaData))]
    public partial class ReadTodayFiles
    {
    }
    
    public partial class ReadTodayFilesMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid AnnouncementID { get; set; }
        [Required]
        public System.Guid DownloadFilesID { get; set; }
    
        public virtual DownloadFile DownloadFiles { get; set; }
        public virtual ReadToday ReadToday { get; set; }
    }
}
