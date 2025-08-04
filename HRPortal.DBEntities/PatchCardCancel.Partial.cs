namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PatchCardCancelMetaData))]
    public partial class PatchCardCancel
    {
    }
    
    public partial class PatchCardCancelMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string FormNo { get; set; }
        [Required]
        public int Status { get; set; }
        public string CancelReason { get; set; }
        [Required]
        public System.Guid Createdby { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> Modifiedby { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }
        [Required]
        public System.Guid PatchCardFormID { get; set; }
    
        public virtual Employee Employees { get; set; }
        public virtual Employee Employees1 { get; set; }
        public virtual Employee Employees2 { get; set; }
        public virtual PatchCardForm PatchCardForm { get; set; }
    }
}
