namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(OverTimeFormMetaData))]
    public partial class OverTimeForm
    {
    }

    public partial class OverTimeFormMetaData
    {
        [Required]
        public System.Guid ID { get; set; }

        [StringLength(20, ErrorMessage = "欄位長度不得大於 20 個字元")]
        [Required]
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
        [Display(Name = "起始時間")]
        public System.DateTime StartTime { get; set; }
        [Required]
        [Display(Name = "結束時間")]
        public System.DateTime EndTime { get; set; }
        [Required]
        public decimal OverTimeAmount { get; set; }

        [StringLength(10, ErrorMessage = "欄位長度不得大於 10 個字元")]
        public string OverTimeReasonCode { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        [Display(Name = "Text_OvertimeReason", ResourceType = typeof(HRPortal.MultiLanguage.Resource))] //[Display(Name ="加班原因")]
        public string OverTimeReason { get; set; }
        [Required]
        public short CompensationWay { get; set; }
        [Required]
        public bool HaveDinning { get; set; }
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

        public virtual Company Company { get; set; }
        public virtual Department Department { get; set; }
        public virtual Employee Creator { get; set; }
        public virtual Employee Deleter { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual ICollection<OverTimeCancel> OverTimeCancels { get; set; }

    }
}