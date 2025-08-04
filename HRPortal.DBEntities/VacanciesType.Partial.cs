namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(VacanciesTypeMetaData))]
    public partial class VacanciesType
    {
    }

    public partial class VacanciesTypeMetaData
    {
        [Required]
        public System.Guid Id { get; set; }

        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Createdby")]
        public System.Guid Createdby { get; set; }
        [Required]
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "CreatedTime")]
        public System.DateTime CreatedTime { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "Modifiedby")]
        public Nullable<System.Guid> Modifiedby { get; set; }
        [Display(ResourceType = typeof(MultiLanguage.Resource), Name = "ModifiedTime")]
        public Nullable<System.DateTime> ModifiedTime { get; set; }

        public virtual ICollection<Vacancies> Vacancies { get; set; }
    }
}
