using HRPortal.DBEntities;
using HRPortal.SignFlow.SignLists;
using System;
using System.Linq;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Model
{
    public class HRPotralFormSignStatus
    {
        public bool Role { get; set; }
        public string SignerEmployeeEnglishName { get; set; }
        public string SenderEmployeeEnglishName { get; set; }
        public string getLanguageCookie { get; set; }
        public string SignFlowID { get; set; }
        public string FormNo { get; set; }
        public FormType FormType { get; set; }
        public int FormStatus { get; set; }
        public string FormSummary { get; set; }
        public DateTime FormCreateDate { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string SenderEmployeeNo { get; set; }
        public string SenderEmployeeName { get; set; }
        public string SignerEmployeeNo { get; set; }
        public string SignerEmployeeName { get; set; }
        public string SignOrder { get; set; }
        public string SignStatus { get; set; }
        public string SignType { get; set; }
        public int UrgentOrder { get; set; }
        public int FormNoOrder { get; set; }
        public string FilePath { get; set; }
        public DateTime BeDate { get; set; }

        public string AbsentCode { get; set; }
        public string AbsentName{get;set;}
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Amount { get; set; }
        public string AbsentUnit { get; set; }
        public string SalaryFormNo { get; set; }
        public DateTime? LeaveDate { get; set; }

        
        //20190520 Daniel 增加英文部門與假別名稱
        public string DepartmentEnglishName { get; set; }
        public string AbsentEnglishName { get; set; }

        public HRPotralFormSignStatus()
        { 
        
        }

        public HRPotralFormSignStatus(SignFlowRecModel signFlow)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                FormType = (SignLists.FormType)Enum.Parse(typeof(SignLists.FormType), signFlow.FormType);

                switch (FormType)
                {
                    case FormType.Leave:
                        LeaveForm _leaveForm = _db.LeaveForms.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _leaveForm.FormNo;
                        FormStatus = _leaveForm.Status;
                        FormCreateDate = _leaveForm.CreatedTime.Date;
                        CompanyCode = _leaveForm.Company.CompanyCode;
                        CompanyName = _leaveForm.Company.CompanyName;
                        DepartmentCode = _leaveForm.Department.DepartmentCode;
                        DepartmentName = _leaveForm.Department.DepartmentName;
                        SenderEmployeeNo = _leaveForm.Employee.EmployeeNO;
                        SenderEmployeeName = _leaveForm.Employee.EmployeeName;
                        LeaveDate = _leaveForm.Employee.LeaveDate;
                        AbsentCode = _leaveForm.AbsentCode;
                        DepartmentEnglishName = _leaveForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _leaveForm.Employee.EmployeeEnglishName;
                        break;
                    case FormType.OverTime:
                        OverTimeForm _overTimeForm = _db.OverTimeForms.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _overTimeForm.FormNo;
                        FormStatus = _overTimeForm.Status;
                        FormCreateDate = _overTimeForm.CreatedTime.Date;
                        CompanyCode = _overTimeForm.Company.CompanyCode;
                        CompanyName = _overTimeForm.Company.CompanyName;
                        DepartmentCode = _overTimeForm.Department.DepartmentCode;
                        DepartmentName = _overTimeForm.Department.DepartmentName;
                        SenderEmployeeNo = _overTimeForm.Employee.EmployeeNO;
                        SenderEmployeeName = _overTimeForm.Employee.EmployeeName;
                        LeaveDate = _overTimeForm.Employee.LeaveDate;
                        DepartmentEnglishName = _overTimeForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _overTimeForm.Employee.EmployeeEnglishName;
                        break;
                    case FormType.PatchCard:
                        PatchCardForm _patchCardForm = _db.PatchCardForms.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _patchCardForm.FormNo;
                        FormStatus = _patchCardForm.Status;
                        FormCreateDate = _patchCardForm.CreatedTime.Date;
                        CompanyCode = _patchCardForm.Company.CompanyCode;
                        CompanyName = _patchCardForm.Company.CompanyName;
                        DepartmentCode = _patchCardForm.Department.DepartmentCode;
                        DepartmentName = _patchCardForm.Department.DepartmentName;
                        SenderEmployeeNo = _patchCardForm.Employee.EmployeeNO;
                        SenderEmployeeName = _patchCardForm.Employee.EmployeeName;
                        LeaveDate = _patchCardForm.Employee.LeaveDate;
                        DepartmentEnglishName = _patchCardForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _patchCardForm.Employee.EmployeeEnglishName;
                        break;
                    case FormType.LeaveCancel:
                        LeaveCancel _leaveCancel = _db.LeaveCancels.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _leaveCancel.FormNo;
                        FormStatus = _leaveCancel.Status;
                        FormCreateDate = _leaveCancel.CreatedTime.Date;
                        CompanyCode = _leaveCancel.LeaveForm.Company.CompanyCode;
                        CompanyName = _leaveCancel.LeaveForm.Company.CompanyName;
                        DepartmentCode = _leaveCancel.LeaveForm.Department.DepartmentCode;
                        DepartmentName = _leaveCancel.LeaveForm.Department.DepartmentName;
                        SenderEmployeeNo = _leaveCancel.LeaveForm.Employee.EmployeeNO;
                        SenderEmployeeName = _leaveCancel.LeaveForm.Employee.EmployeeName;
                        LeaveDate = _leaveCancel.LeaveForm.Employee.LeaveDate;
                        AbsentCode = _leaveCancel.LeaveForm.AbsentCode;
                        DepartmentEnglishName = _leaveCancel.LeaveForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _leaveCancel.LeaveForm.Employee.EmployeeEnglishName;
                        break;
                    case FormType.OverTimeCancel:
                        OverTimeCancel _overTimeCancel = _db.OverTimeCancels.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _overTimeCancel.FormNo;
                        FormStatus = _overTimeCancel.Status;
                        FormCreateDate = _overTimeCancel.CreatedTime.Date;
                        CompanyCode = _overTimeCancel.OverTimeForm.Company.CompanyCode;
                        CompanyName = _overTimeCancel.OverTimeForm.Company.CompanyName;
                        DepartmentCode = _overTimeCancel.OverTimeForm.Department.DepartmentCode;
                        DepartmentName = _overTimeCancel.OverTimeForm.Department.DepartmentName;
                        SenderEmployeeNo = _overTimeCancel.OverTimeForm.Employee.EmployeeNO;
                        SenderEmployeeName = _overTimeCancel.OverTimeForm.Employee.EmployeeName;
                        LeaveDate = _overTimeCancel.OverTimeForm.Employee.LeaveDate;
                        DepartmentEnglishName = _overTimeCancel.OverTimeForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _overTimeCancel.OverTimeForm.Employee.EmployeeEnglishName;
                        break;
                    case FormType.PatchCardCancel:
                        PatchCardCancel _patchCardCancel = _db.PatchCardCancel.FirstOrDefault(x => x.FormNo == signFlow.FormNumber);
                        FormNo = _patchCardCancel.FormNo;
                        FormStatus = _patchCardCancel.Status;
                        FormCreateDate = _patchCardCancel.CreatedTime.Date;
                        CompanyCode = _patchCardCancel.PatchCardForm.Company.CompanyCode;
                        CompanyName = _patchCardCancel.PatchCardForm.Company.CompanyName;
                        DepartmentCode = _patchCardCancel.PatchCardForm.Department.DepartmentCode;
                        DepartmentName = _patchCardCancel.PatchCardForm.Department.DepartmentName;
                        SenderEmployeeNo = _patchCardCancel.PatchCardForm.Employee.EmployeeNO;
                        SenderEmployeeName = _patchCardCancel.PatchCardForm.Employee.EmployeeName;
                        LeaveDate = _patchCardCancel.PatchCardForm.Employee.LeaveDate;
                        DepartmentEnglishName = _patchCardCancel.PatchCardForm.Department.DepartmentEnglishName;
                        SenderEmployeeEnglishName = _patchCardCancel.PatchCardForm.Employee.EmployeeEnglishName;
                        break;
                    default:
                        throw new Exception("FormTypeError: " + signFlow.FormType);
                        break;
                }

                SignFlowID = signFlow.ID;
                SignerEmployeeNo = signFlow.SignerID;
                string signCompanyCode = CompanyCode;
                if (signFlow.SignCompanyID != null) {
                    signCompanyCode = signFlow.SignCompanyID;
                }
                if (signFlow.SignType == "D")
                {
                    Department dept = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == signCompanyCode && x.DepartmentCode == signFlow.SignerID);
                    SignerEmployeeName = dept.DepartmentName;
                    SignerEmployeeEnglishName = dept.DepartmentEnglishName;
                    //SignerEmployeeName = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == signCompanyCode && x.DepartmentCode == signFlow.SignerID).DepartmentName;
                }
                else
                {
                    if (signFlow.SignerID != null)
                    {
                        Employee emp = _db.Employees.FirstOrDefault(x => x.Company.CompanyCode == signCompanyCode && x.EmployeeNO == signFlow.SignerID);
                        SignerEmployeeName = emp.EmployeeName;
                        SignerEmployeeEnglishName = emp.EmployeeEnglishName;
                        //SignerEmployeeName = _db.Employees.FirstOrDefault(x => x.Company.CompanyCode == signCompanyCode && x.EmployeeNO == signFlow.SignerID).EmployeeName;
                        
                    }
                    else
                    {
                        SignerEmployeeName = null;
                        SignerEmployeeEnglishName = null;
                    }
                }
                SignOrder = signFlow.SignOrder;
                SignStatus = signFlow.SignStatus;
                SignType = signFlow.SignType;
                UrgentOrder = signFlow.SignType == "S" ? 0 : signFlow.SignStatus == "W" ? 1 : 2;
            }
        }
    }
}
