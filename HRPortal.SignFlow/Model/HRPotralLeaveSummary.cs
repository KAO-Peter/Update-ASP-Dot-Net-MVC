using HRPortal.DBEntities;
using HRPortal.SignFlow.SignLists;
using System;
using System.Linq;

using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Model
{
    public class HRPotralLeaveSummary
    {
        public bool Role { get; set; }
        public string FormNo { get; set; }
        public string AbsentCode { get; set; }
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
        public string SenderEmployeeNameEn { get; set; }
        public string SignerEmployeeNo { get; set; }
        public string SignStatus { get; set; }
        public string SignType { get; set; }
        public int UrgentOrder { get; set; }
        public int FormNoOrder { get; set; }
        public string AbsentName { get; set; }
        public string AbsentNameEn { get; set; }
        public decimal LeaveAmount { get; set; }
        public decimal TotalLeaveAmount { get; set; }
        public decimal TotalUseAmount { get; set; }
        public decimal TotalAnnualAmount { get; set; }
        public string AbsentUnit { get; set; }

        public HRPotralLeaveSummary()
        {
        }

        public HRPotralLeaveSummary(SignFlowRecModel signFlow)
        {
            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                FormType = (SignLists.FormType)Enum.Parse(typeof(SignLists.FormType), signFlow.FormType);
                if (FormType == FormType.Leave)
                {
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
                    SenderEmployeeNameEn = _leaveForm.Employee.EmployeeEnglishName;
                    AbsentCode = _leaveForm.AbsentCode;
                    LeaveAmount = _leaveForm.LeaveAmount;
                    FormSummary = string.Format("{0} ~ {1}", _leaveForm.StartTime.ToString("yyyy/MM/dd HH:mm"), _leaveForm.EndTime.ToString("yyyy/MM/dd HH:mm"));
                    SignStatus = signFlow.SignStatus;
                    SignType = signFlow.SignType;
                    SignerEmployeeNo = signFlow.SignerID;
                    UrgentOrder = signFlow.SignType == "S" ? 0 : signFlow.SignStatus == "W" ? 1 : 2;
                }
                else
                {
                    throw new Exception("FormTypeError: " + signFlow.FormType);
                }
            }
        }
    }
}