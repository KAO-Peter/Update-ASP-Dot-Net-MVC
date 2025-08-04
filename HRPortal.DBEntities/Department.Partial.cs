namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(DepartmentMetaData))]
    public partial class Department
    {
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.Guid> SignManagerId1 { get; set; }
        public System.Guid EffectiveDateID { get; set; }
    }

    public partial class DepartmentMetaData
    {
        [Required]
        public System.Guid ID { get; set; }

        [StringLength(12, ErrorMessage = "欄位長度不得大於 12 個字元")]
        [Required]
        public string DepartmentCode { get; set; }

        [StringLength(64, ErrorMessage = "欄位長度不得大於 64 個字元")]
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "DepartmentName")]
        public string DepartmentName { get; set; }

        [StringLength(64, ErrorMessage = "欄位長度不得大於 64 個字元")]
        public string DepartmentEnglishName { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        public Nullable<System.Guid> ParentDepartmentID { get; set; }
        public Nullable<System.Guid> SignParentID { get; set; }
        public Nullable<System.Guid> ManagerId { get; set; }
        public Nullable<System.Guid> SignManagerId { get; set; }
        [Required]
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        //[Required]
        public bool OnlyForSign { get; set; }

        public virtual ICollection<Department> Child { get; set; }
        public virtual Department Parent { get; set; }
        public virtual ICollection<Department> SignChild { get; set; }
        public virtual Department SignParent { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual Employee Manager { get; set; }
        public virtual Employee SignManager { get; set; }
        public virtual ICollection<LeaveForm> LeaveForms { get; set; }
        public virtual ICollection<OverTimeForm> OverTimeForms { get; set; }
        public virtual ICollection<PatchCardForm> PatchCardForms { get; set; }
        public virtual ICollection<AnswerFAQ> AnswerFAQs { get; set; }
        public virtual ICollection<Employee> Employees1 { get; set; }
    }
}