using System;
using System.Linq;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Model
{
    public class HRPotralSignFlowStatus
    {
        public string getLanguageCookie { get; set; }
        public string ActSignerEmployeeEnglishName { get; set; }
        public string SignerEmployeeEnglishName { get; set; }
        public int OrderNumber { get; set; }
        public string SignFlowID { get; set; }
        public string FormNo { get; set; }
        public string FormLevelID { get; set; }
        public string FormLevelName { get; set; }
        public string SignOrder { get; set; }
        public string SignStatus { get; set; }
        public string SignType { get; set; }
        public string SignerEmployeeNo { get; set; }
        public string SignerEmployeeNo2 { get; set; }
        public string SignerEmployeeName { get; set; }
        public string ActSignerEmployeeNo { get; set; }
        public string ActSignerEmployeeName { get; set; }
        public string Instruction { get; set; }
        public Nullable<System.DateTime> SignDate { get; set; }

        public HRPotralSignFlowStatus(SignFlowRecModel signFlow, Guid companyId)
        {
            SignFlowID = signFlow.ID;
            FormNo = signFlow.FormNumber;
            FormLevelID = signFlow.FormLevelID;
            SignOrder = signFlow.FormLevelID;
            SignStatus = signFlow.SignStatus;
            SignType = signFlow.SignType;
            //變成為原簽核人員
            SignerEmployeeNo2 = signFlow.SignerID;
            SignerEmployeeNo = signFlow.OrgSignerID;
            ActSignerEmployeeNo = signFlow.ActSignerID;
            Instruction = signFlow.Instruction;
            SignDate = signFlow.SignDate;

            using (HRPortal.DBEntities.NewHRPortalEntities _db = new DBEntities.NewHRPortalEntities())
            {
                if (!string.IsNullOrEmpty(SignerEmployeeNo))
                {
                    if (signFlow.SignType == "D")
                    {
                        if (signFlow.SignCompanyID == null)
                        {
                            SignerEmployeeName = _db.Departments.FirstOrDefault(x => x.CompanyID == companyId && x.DepartmentCode == SignerEmployeeNo).DepartmentName;
                        }
                        else
                        {
                            SignerEmployeeName = _db.Departments.FirstOrDefault(x => x.Company.CompanyCode == signFlow.SignCompanyID && x.DepartmentCode == SignerEmployeeNo).DepartmentName;
                        }
                    }
                    else
                    {
                        if (signFlow.SignCompanyID == null)
                        {
                            SignerEmployeeName = _db.Employees.FirstOrDefault(x => x.CompanyID == companyId && x.EmployeeNO == SignerEmployeeNo).EmployeeName;
                        }
                        else
                        {
                            SignerEmployeeName = _db.Employees.FirstOrDefault(x => x.Company.CompanyCode == signFlow.SignCompanyID && x.EmployeeNO == SignerEmployeeNo).EmployeeName;
                        }

                    }
                }

                if (!string.IsNullOrEmpty(ActSignerEmployeeNo))
                {
                    if (signFlow.SignCompanyID == null)
                    {
                        ActSignerEmployeeName = _db.Employees.FirstOrDefault(x => x.CompanyID == companyId && x.EmployeeNO == ActSignerEmployeeNo).EmployeeName;
                    }
                    else {
                        //修改跨公司簽核紀錄資料抓取不到值的Bug Irving 20161129
                      var  ActSignerEmployeeDate = _db.Employees.Where(x => x.Company.CompanyCode == signFlow.SignCompanyID && x.EmployeeNO == ActSignerEmployeeNo).ToList();
                      if (ActSignerEmployeeDate.Count == 0)
                      {
                          ActSignerEmployeeName = _db.Employees.FirstOrDefault(x => x.CompanyID == companyId && x.EmployeeNO == ActSignerEmployeeNo).EmployeeName;
                      }
                      else
                      {
                          ActSignerEmployeeName = _db.Employees.FirstOrDefault(x => x.Company.CompanyCode == signFlow.SignCompanyID && x.EmployeeNO == ActSignerEmployeeNo).EmployeeName;
                      }
                        //End
                    }
                    
                }
            }
        }
    }
}
