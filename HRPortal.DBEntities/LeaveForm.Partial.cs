namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(LeaveFormMetaData))]
    public partial class LeaveForm
    {
    }

    public partial class LeaveFormMetaData
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

        /// <summary>
        /// 假別代碼
        /// </summary>
        [StringLength(10, ErrorMessage = "欄位長度不得大於 10 個字元")]
        //[Required]
        public string AbsentCode { get; set; }
        [Required]
        public decimal AbsentAmount { get; set; }
        [StringLength(1, ErrorMessage = "欄位長度不得大於 1 個字元")]
        //[Required]
        public string AbsentUnit { get; set; }
        [Display(Name = "起始時間")]
        [Required]
        public System.DateTime StartTime { get; set; }
        [Required]
        [Display(Name ="結束時間")]
        public System.DateTime EndTime { get; set; }
       
        public decimal LeaveAmount { get; set; }
        [Required]
        public decimal AfterAmount { get; set; }
        [StringLength(100, ErrorMessage = "欄位長度不得大於 100 個字元")]
        public string LeaveReason { get; set; }
        public Nullable<System.Guid> AgentID { get; set; }

        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string FilePath { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string FileName { get; set; }

        [StringLength(200, ErrorMessage = "欄位長度不得大於 200 個字元")]
        public string FileFormat { get; set; }
        [Required]
        [Display(Name = "是否出國")]
        public bool IsAbroad { get; set; }
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
        public virtual Employee Agent { get; set; }
        public virtual Employee Creator { get; set; }
        public virtual Employee Deleter { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual ICollection<LeaveCancel> LeaveCancels { get; set; }
    }
}