namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(SerialControlMetaData))]
    public partial class SerialControl
    {
    }
    
    public partial class SerialControlMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string SerialName { get; set; }
        [Required]
        public int SerialNumber { get; set; }
    }
}
