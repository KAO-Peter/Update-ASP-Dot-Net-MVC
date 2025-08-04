using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRPortal.EffectiveDateSchedule
{
    public class EffectiveDateTask : IDisposable
    {
        public Action<string> OnMessage;
        //private Guid _userRoleId;

        private NewHRPortalEntities _db;
        public NewHRPortalEntities DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new NewHRPortalEntities();
                }
                return _db;
            }
        }

        public EffectiveDateTask()
        {
        }

        public EffectiveDateTask(NewHRPortalEntities db)
        {
            this._db = db;
        }

        public void WriteMessage(string message)
        {
            if (OnMessage != null)
            {
                OnMessage(message);
            }
        }

        public void Dispose()
        {
        }

        public async Task Run()
        {
            List<Employee> _employeeList = DB.Employees.ToList();
            List<EffectiveDate> _EffectiveDateList = DB.EffectiveDate.ToList();
            List<Department> _DepartmentData = DB.Departments.ToList();
            List<EffectiveDate> _EffectiveData;
            List<EffectiveDate> _DepartmentsList;
            List<EffectiveDate> _EmployeeList;
            //讀取待變更資料 1：部門管理、2：簽核部門 x.Function_Type == 1 && 
            string checkDate = DateTime.Now.ToShortDateString() + " 23:59:59";
            _EffectiveData = _EffectiveDateList.Where(x => x.Change_Type == "N" && DateTime.Parse(x.Change_Date.Value + "") <= Convert.ToDateTime(checkDate)).ToList();

            if (_EffectiveData.Count == 0)
            {
                WriteMessage("無資料需變更");
            }
            else
            {
                int sumDep = 0, sumEmp = 0;
                //1：部門管理
                _DepartmentsList = _EffectiveData.Where(x => x.Function_Type == 1).ToList();

                for (int i = 0; i <= +_DepartmentsList.Count - 1; i++)
                {
                    var updDepartment = _db.Departments.Find(_DepartmentsList[i].Function_ID);
                    updDepartment.SignManagerId = (Guid)_DepartmentsList[i].Change_ID;
                    _db.SaveChanges();

                    updEffectiveDate(_DepartmentsList[i].ID);

                    WriteMessage(string.Format("{0}：已變更簽核主管", updDepartment.DepartmentName));
                    sumDep = sumDep + 1;
                }
                WriteMessage(string.Format("部門管理：已變更簽核主管共 {0} 筆", sumDep));

                //2：簽核部門
                _EmployeeList = _EffectiveData.Where(x => x.Function_Type == 2).ToList();

                for (int i = 0; i <= +_EmployeeList.Count - 1; i++)
                {
                    var updEmployees = _db.Employees.Find(_EmployeeList[i].Function_ID);
                    updEmployees.SignDepartmentID = (Guid)_EmployeeList[i].Change_ID;
                    _db.SaveChanges();

                    updEffectiveDate(_EmployeeList[i].ID);

                    WriteMessage(string.Format("{0}-{1}：已變更簽核部門", updEmployees.EmployeeNO, updEmployees.EmployeeName));
                    sumEmp = sumEmp + 1;
                }
                WriteMessage(string.Format("簽核部門：已變更簽核部門共 {0} 筆", sumEmp));
            }
        }

        /// <summary>
        /// 變更生效日期狀態
        /// </summary>
        /// <param name="id"></param>
        private void updEffectiveDate(Guid id)
        {
            var updEffectiveDate = _db.EffectiveDate.Find(id);
            updEffectiveDate.Change_Type = "Y";
            _db.SaveChanges();
        }
    }
}
