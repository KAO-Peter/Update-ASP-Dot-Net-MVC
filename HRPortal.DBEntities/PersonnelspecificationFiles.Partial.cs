namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PersonnelspecificationFilesMetaData))]
    public partial class PersonnelspecificationFiles
    {
    }
    
    public partial class PersonnelspecificationFilesMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid AnnouncementID { get; set; }
        [Required]
        public System.Guid DownloadFilesID { get; set; }
    
        public virtual DownloadFile DownloadFiles { get; set; }
        public virtual Personnelspecification Personnelspecification { get; set; }
    }
}
