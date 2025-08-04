namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(DeptCostMetaData))]
    public partial class DeptCost
    {
    }
    
    public partial class DeptCostMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        [Required]
        public int Dept_id { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string DeptCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string DeptName { get; set; }
        [Required]
        public int Cost_id { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string CostCode { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string CostName { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
    
        public virtual Company Company { get; set; }
    }
}
