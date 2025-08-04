using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class CasualFormViewModel
    {
        public IEnumerable<SelectListItem> DepartmentList { get; set; }
        public IEnumerable<SelectListItem> DeptCostList { get; set; }
        public IEnumerable<SelectListItem> SalaryUnitList { get; set; }
        public IEnumerable<SelectListItem> ClassList { get; set; }
        public IEnumerable<SelectListItem> TimeList { get; set; }

        public CasualForm casualFormData { get; set; }

        public double Total { get; set; }
        public double SumWorkHours { get; set; }
        public string PrevSalaryUnit { get; set; }
        public double PrevSalary { get; set; }
        public bool PrevStatus { get; set; }
        public string LockFlag { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }

        public CasualFormViewModel()
        {
            DepartmentList = null;
            DeptCostList = null;
            SalaryUnitList = null;
            ClassList = null;

            casualFormData = new CasualForm();
            casualFormData.ExcuteDate = DateTime.Now;
            casualFormData.Salary = 0;
            casualFormData.DefaultStartTime = "";
            casualFormData.DefaultEndTime = "";
            casualFormData.DefaultWorkHours = 0;
            casualFormData.StartTime = "";
            casualFormData.EndTime = "";
            casualFormData.WorkHours = 0;
            casualFormData.DiningHours = 0;
            casualFormData.Allowance = 0;
            casualFormData.CashFlag = false;
            casualFormData.Status = "suit";  //勾選=only，未勾選=suit
            casualFormData.CardKeep = false;  //續用=true，歸還=false

            Total = 0;
            SumWorkHours = 0;
            PrevSalaryUnit = "";
            PrevSalary = 0;
            PrevStatus = false;
            LockFlag = "n";
            IsUpdate = true;
            IsDelete = true;
        }
    }
}
