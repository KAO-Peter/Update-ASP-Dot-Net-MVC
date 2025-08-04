namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(CasualFormLogMetaData))]
    public partial class CasualFormLog
    {
    }
    
    public partial class CasualFormLogMetaData
    {
        [Required]
        public System.Guid ID { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string ExcuteEmpNO { get; set; }
        [Required]
        public System.DateTime ExcuteTime { get; set; }
        
        [StringLength(15, ErrorMessage="欄位長度不得大於 15 個字元")]
        [Required]
        public string ChangeType { get; set; }
        [Required]
        public System.Guid CasualForm_ID { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string EmployeeNO { get; set; }
        [Required]
        public int EmpData_ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string EmpName { get; set; }
        [Required]
        public int Dept_ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Dept_Code { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Dept_Name { get; set; }
        [Required]
        public int Cost_ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Cost_Code { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Cost_Name { get; set; }
        [Required]
        public System.DateTime ExcuteDate { get; set; }
        [Required]
        public int Class_ID { get; set; }
        
        [StringLength(50, ErrorMessage="欄位長度不得大於 50 個字元")]
        [Required]
        public string Class_Code { get; set; }
        
        [StringLength(30, ErrorMessage="欄位長度不得大於 30 個字元")]
        [Required]
        public string Class_Name { get; set; }
        
        [StringLength(5, ErrorMessage="欄位長度不得大於 5 個字元")]
        [Required]
        public string DefaultStartTime { get; set; }
        
        [StringLength(5, ErrorMessage="欄位長度不得大於 5 個字元")]
        [Required]
        public string DefaultEndTime { get; set; }
        [Required]
        public double DefaultWorkHours { get; set; }
        
        [StringLength(5, ErrorMessage="欄位長度不得大於 5 個字元")]
        public string StartTime { get; set; }
        
        [StringLength(5, ErrorMessage="欄位長度不得大於 5 個字元")]
        public string EndTime { get; set; }
        public Nullable<double> DiningHours { get; set; }
        public Nullable<double> WorkHours { get; set; }
        
        [StringLength(1, ErrorMessage="欄位長度不得大於 1 個字元")]
        public string SalaryUnit { get; set; }
        public Nullable<double> Salary { get; set; }
        public Nullable<double> Allowance { get; set; }
        
        [StringLength(20, ErrorMessage="欄位長度不得大於 20 個字元")]
        public string CardNo { get; set; }
        public Nullable<bool> CardKeep { get; set; }
        public Nullable<bool> CashFlag { get; set; }
        public Nullable<double> DaySalary { get; set; }
        
        [StringLength(7, ErrorMessage="欄位長度不得大於 7 個字元")]
        public string Status { get; set; }
        
        [StringLength(15, ErrorMessage="欄位長度不得大於 15 個字元")]
        public string CasualFormNo { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        public string UpdateEmployeeNO { get; set; }
        [Required]
        public System.DateTime CreateDate { get; set; }
        
        [StringLength(16, ErrorMessage="欄位長度不得大於 16 個字元")]
        [Required]
        public string CreateEmployeeNO { get; set; }
        [Required]
        public System.Guid CompanyID { get; set; }
    
        public virtual Company Companys { get; set; }
    }
}
