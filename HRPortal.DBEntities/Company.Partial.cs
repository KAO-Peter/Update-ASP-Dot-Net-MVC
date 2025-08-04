namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(CompanyMetaData))]
    public partial class Company
    {
    }

    public partial class CompanyMetaData
    {
        [Required]
        public System.Guid ID { get; set; }

        [StringLength(10, ErrorMessage = "欄位長度不得大於 10 個字元")]
        [Required]
        public string CompanyCode { get; set; }

        [StringLength(64, ErrorMessage = "欄位長度不得大於 64 個字元")]
        [Required]
        public string CompanyName { get; set; }
        public Nullable<System.Guid> ParentCompanyID { get; set; }
        [Required]
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }

        public virtual ICollection<Company> Child { get; set; }
        public virtual Company Parent { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<LeaveForm> LeaveForms { get; set; }
        public virtual ICollection<OverTimeForm> OverTimeForms { get; set; }
        public virtual ICollection<PatchCardForm> PatchCardForms { get; set; }
        public virtual ICollection<AnswerFAQ> AnswerFAQs { get; set; }
    }
}