namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaCompanyMetaData))]
    public partial class PfaCompany
    {
    }
    
    public partial class PfaCompanyMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(10, ErrorMessage="欄位長度不得大於 10 個字元")]
        [Required]
        public string CompanyCode { get; set; }
        
        [StringLength(64, ErrorMessage="欄位長度不得大於 64 個字元")]
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
        public Nullable<int> Company_ID { get; set; }
        public Nullable<bool> MainFlag { get; set; }
        
        [StringLength(100, ErrorMessage="欄位長度不得大於 100 個字元")]
        public string ContactPrincipal { get; set; }
    
        public virtual ICollection<PfaCompany> Companys1 { get; set; }
        public virtual PfaCompany Companys2 { get; set; }
        public virtual ICollection<PfaDepartment> Departments { get; set; }
        public virtual ICollection<PfaEmployee> Employees { get; set; }
        public virtual ICollection<PfaDept> PfaDept { get; set; }
        public virtual ICollection<PfaOrg> PfaOrg { get; set; }
        public virtual ICollection<PfaEmpType> PfaEmpType { get; set; }
        public virtual ICollection<PfaAbility> PfaAbility { get; set; }
        public virtual ICollection<PfaIndicator> PfaIndicator { get; set; }
        public virtual ICollection<PfaCycle> PfaCycle { get; set; }
    }
}
