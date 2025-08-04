namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(C__MigrationHistoryMetaData))]
    public partial class C__MigrationHistory
    {
    }
    
    public partial class C__MigrationHistoryMetaData
    {
        
        [StringLength(150, ErrorMessage="欄位長度不得大於 150 個字元")]
        [Required]
        public string MigrationId { get; set; }
        
        [StringLength(300, ErrorMessage="欄位長度不得大於 300 個字元")]
        [Required]
        public string ContextKey { get; set; }
        [Required]
        public byte[] Model { get; set; }
        
        [StringLength(32, ErrorMessage="欄位長度不得大於 32 個字元")]
        [Required]
        public string ProductVersion { get; set; }
    }
}
