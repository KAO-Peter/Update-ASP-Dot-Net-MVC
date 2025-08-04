namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(AnswerVacanciesMetaData))]
    public partial class AnswerVacancies
    {
    }

    public partial class AnswerVacanciesMetaData
    {

        public System.Guid ID { get; set; }

        //[StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        //[Required]
        //[Display(Name = "標題")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "詢問內容")]
        public string ContentText { get; set; }
        [Display(Name = "回覆")]
        public string Reply { get; set; }

        public int Status { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid DepartmentID { get; set; }
        [Required]
        public System.Guid Createdby { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> Modifiedby { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }

        public virtual Company Company { get; set; }
        public virtual Employee Creator { get; set; }
        public virtual Department Department { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Employee Modifier { get; set; }
        public virtual Employee Respondents { get; set; }
    }
}
