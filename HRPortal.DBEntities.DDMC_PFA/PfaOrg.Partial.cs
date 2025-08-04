namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaOrgMetaData))]
    public partial class PfaOrg
    {
    }
    
    public partial class PfaOrgMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaOrgCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string PfaOrgName { get; set; }
        public Nullable<System.Guid> OrgManagerId { get; set; }
        public Nullable<int> Ordering { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.Guid> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedTime { get; set; }
    
        public virtual PfaCompany Companys { get; set; }
        public virtual PfaEmployee Employees { get; set; }
        public virtual ICollection<PfaOrgDept> PfaOrgDept { get; set; }
        public virtual ICollection<PfaCycleRation> PfaCycleRation { get; set; }
        public virtual ICollection<PfaCycleEmp> PfaCycleEmp { get; set; }
    }
}
