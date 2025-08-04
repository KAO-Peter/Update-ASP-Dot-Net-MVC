namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(SignoffAgentsMetaData))]
    public partial class SignoffAgents
    {
    }
    
    public partial class SignoffAgentsMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(10, ErrorMessage="欄位長度不得大於 10 個字元")]
        [Required]
        public string CompanyCode { get; set; }
        
        [StringLength(12, ErrorMessage="欄位長度不得大於 12 個字元")]
        [Required]
        public string SignDepartmentCode { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string EmployeeNO { get; set; }
        
        [StringLength(12, ErrorMessage="欄位長度不得大於 12 個字元")]
        public string AgentDep_ID1 { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string AgentEmp_ID1 { get; set; }
        
        [StringLength(12, ErrorMessage="欄位長度不得大於 12 個字元")]
        public string AgentDep_ID2 { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string AgentEmp_ID2 { get; set; }
        
        [StringLength(12, ErrorMessage="欄位長度不得大於 12 個字元")]
        public string AgentDep_ID3 { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        public string AgentEmp_ID3 { get; set; }
    }
}
