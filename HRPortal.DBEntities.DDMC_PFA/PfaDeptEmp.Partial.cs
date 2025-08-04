namespace HRPortal.DBEntities.DDMC_PFA
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(PfaDeptEmpMetaData))]
    public partial class PfaDeptEmp
    {
    }
    
    public partial class PfaDeptEmpMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid PfaDeptID { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
    
        public virtual PfaDept PfaDept { get; set; }
    }
}
