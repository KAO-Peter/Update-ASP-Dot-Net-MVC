namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(VacanciesMetaData))]
    public partial class Vacancies
    {
    }

    public partial class VacanciesMetaData
    {
        //[Required]
        public System.Guid Id { get; set; }

        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "FAQContentText")]
        public string ContentText { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "FAQType")]
        public System.Guid Type { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Createdby")]
        public System.Guid Createdby { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "CreatedTime")]
        public System.DateTime CreatedTime { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Modifiedby")]
        public Nullable<System.Guid> Modifiedby { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "ModifiedTime")]
        public Nullable<System.DateTime> ModifiedTime { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.Guid> Deletedby { get; set; }
        public Nullable<System.DateTime> DeletedTime { get; set; }

        /// <summary>
        /// Createby
        /// </summary>
        public virtual Employee Employee { get; set; }
        /// <summary>
        /// Modifiedby
        /// </summary>
        public virtual Employee Employee1 { get; set; }
        public virtual VacanciesType VacanciesType { get; set; }
    }
}
