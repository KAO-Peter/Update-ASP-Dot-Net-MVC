namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSignerListMetaData))]
    public partial class PfaSignerList
    {
    }
    
    public partial class PfaSignerListMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaCycleID { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        [Required]
        public string SignType { get; set; }
        public Nullable<System.DateTime> FirstDate { get; set; }
        public Nullable<System.DateTime> SecondDate { get; set; }
        public Nullable<System.DateTime> LastDate { get; set; }
        [Required]
        public bool IsMeat { get; set; }
    
        public virtual PfaEmployee Employees { get; set; }
        public virtual PfaCycle PfaCycle { get; set; }
    }
}
