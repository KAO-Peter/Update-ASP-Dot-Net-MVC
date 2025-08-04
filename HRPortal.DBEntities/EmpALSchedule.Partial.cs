namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(EmpALScheduleMetaData))]
    public partial class EmpALSchedule
    {
    }
    
    public partial class EmpALScheduleMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
        [Required]
        public System.Guid DepartmentID { get; set; }
        [Required]
        public System.Guid EmployeeID { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public double S01 { get; set; }
        [Required]
        public double S02 { get; set; }
        [Required]
        public double S03 { get; set; }
        [Required]
        public double S04 { get; set; }
        [Required]
        public double S05 { get; set; }
        [Required]
        public double S06 { get; set; }
        [Required]
        public double S07 { get; set; }
        [Required]
        public double S08 { get; set; }
        [Required]
        public double S09 { get; set; }
        [Required]
        public double S10 { get; set; }
        [Required]
        public double S11 { get; set; }
        [Required]
        public double S12 { get; set; }
        [Required]
        public System.DateTime CreatedTime { get; set; }
        [Required]
        public System.DateTime ModifiedTime { get; set; }
        [Required]
        public double D01 { get; set; }
        [Required]
        public double D02 { get; set; }
        [Required]
        public double D03 { get; set; }
        [Required]
        public double D04 { get; set; }
        [Required]
        public double D05 { get; set; }
        [Required]
        public double D06 { get; set; }
        [Required]
        public double D07 { get; set; }
        [Required]
        public double D08 { get; set; }
        [Required]
        public double D09 { get; set; }
        [Required]
        public double D10 { get; set; }
        [Required]
        public double D11 { get; set; }
        [Required]
        public double D12 { get; set; }
    
        public virtual Company Companys { get; set; }
        public virtual Department Departments { get; set; }
        public virtual Employee Employees { get; set; }
    }
}
