namespace HRPortal.DBEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(CasualFormMetaData))]
    public partial class CasualForm
    {
    }

    public partial class CasualFormMetaData
    {
        [Required]
        public System.Guid ID { get; set; }

        [Required]
        public int EmpData_ID { get; set; }

        [Required]
        public int Dept_ID { get; set; }

        [Required]
        public int Cost_ID { get; set; }

        [Required]
        [Display(Name = "日期")]
        public System.DateTime ExcuteDate { get; set; }

        [Required]
        [Display(Name = "班別代碼")]
        public Nullable<int> Class_ID { get; set; }
        public string DefaultStartTime { get; set; }
        public string DefaultEndTime { get; set; }
        public Nullable<double> DefaultWorkHours { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Nullable<double> WorkHours { get; set; }
        public string SalaryUnit { get; set; }

        [Required]
        [Display(Name = "薪資")]
        public Nullable<double> Salary { get; set; }
        public Nullable<double> Allowance { get; set; }

        [Display(Name = "出勤卡號")]
        [StringLength(20, ErrorMessage = "欄位長度不得大於 20 個字元")]
        public string CardNo { get; set; }
        public Nullable<bool> CardKeep { get; set; }
        public Nullable<bool> CashFlag { get; set; }
        public string CasualFormNo { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string UpdateEmployeeNO { get; set; }

        [Required]
        public System.DateTime CreateDate { get; set; }

        [Required]
        public string CreateEmployeeNO { get; set; }

        [Required]
        public System.Guid CompanyID { get; set; }
        public string EmpName { get; set; }

        [Required]
        [Display(Name = "排班部門")]
        public string Dept_Code { get; set; }
        public string Dept_Name { get; set; }

        [Required]
        [Display(Name = "成本中心")]
        public string Cost_Code { get; set; }
        public string Cost_Name { get; set; }
        public string Class_Code { get; set; }
        public string Class_Name { get; set; }

        [Required]
        [Display(Name = "工號")]
        public string EmployeeNO { get; set; }

        public Nullable<double> DaySalary { get; set; }
        public Nullable<double> DiningHours { get; set; }

        public virtual Company Company { get; set; }
    }
}