namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(LeaveCancelMetaData))]
    public partial class LeaveCancel
    {
    }
    
    public partial class LeaveCancelMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        [Required]
        public string FormNo { get; set; }
        public int Status { get; set; }
        [Required]
        [Display(Name = "銷假原因")]
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
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
        public System.Guid LeaveFormID { get; set; }
    
        public virtual Employee Creator { get; set; }
        public virtual Employee Deleter { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual LeaveForm LeaveForm { get; set; }
    }
}
