namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(EmployeeBenefitsFilesMetaData))]
    public partial class EmployeeBenefitsFiles
    {
    }
    
    public partial class EmployeeBenefitsFilesMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid AnnouncementID { get; set; }
        [Required]
        public System.Guid DownloadFilesID { get; set; }
    
        public virtual DownloadFile DownloadFiles { get; set; }
        public virtual EmployeeBenefits EmployeeBenefits { get; set; }
    }
}
