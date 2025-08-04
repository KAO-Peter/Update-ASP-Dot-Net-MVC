namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PatchCardFormMetaData))]
    public partial class PatchCardForm
    {
    }
    
    public partial class PatchCardFormMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        public string FormNo { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid DepartmentID { get; set; }
        [Required]
        [Display(Name ="刷卡時間")]
        public System.DateTime PatchCardTime { get; set; }
        [Required]
        public int Type { get; set; }

        [Required]
        public int ReasonType { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string Reason { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string FilePath { get; set; }
        
        [StringLength(200, ErrorMessage="欄位長度不得大於 200 個字元")]
        public string FileName { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string FileFormat { get; set; }
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
    
        /// <summary>
        /// Create by
        /// </summary>
        public virtual Employee Creator { get; set; }

        /// <summary>
        /// Modified by
        /// </summary>
        public virtual Employee Modifier { get; set; }

        /// <summary>
        /// Deleted by
        /// </summary>
        public virtual Employee Deleter { get; set; }
        /// <summary>
        /// EmployeeID
        /// </summary>
        public virtual Employee Employee { get; set; }

        public virtual Company Company { get; set; }
        public virtual Department Department { get; set; }

    }
}
