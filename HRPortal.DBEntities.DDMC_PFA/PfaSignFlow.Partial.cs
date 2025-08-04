namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaSignFlowMetaData))]
    public partial class PfaSignFlow
    {
    }
    
    public partial class PfaSignFlowMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid SignTypeID { get; set; }
        [Required]
        public int SignStep { get; set; }
        [Required]
        public System.Guid SignLevelID { get; set; }
        public Nullable<System.Guid> DeptClassID { get; set; }
        public Nullable<System.Guid> EmployeesID { get; set; }
        public Nullable<System.Guid> DepartmentsID { get; set; }
        [Required]
        public bool IsSelfEvaluation { get; set; }
        [Required]
        public bool IsFirstEvaluation { get; set; }
        [Required]
        public bool IsSecondEvaluation { get; set; }
        [Required]
        public bool IsUpload { get; set; }
        [Required]
        public System.Guid CreatedBy { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        [Required]
        public bool IsThirdEvaluation { get; set; }
    
        public virtual PfaEmployee Employees { get; set; }
        public virtual PfaDept PfaDept { get; set; }
    }
}
