using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
namespace HRPortal.Services.DDMC_PFA
{
    public class PfaProgressQueryService : BaseCrudService<PfaCycleEmp>
    {
        public PfaProgressQueryService(HRPortal_Services services) : base(services)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtPfaCycleID">批號 ID</param>
        /// <param name="txtPfaOrgID">部門組織 ID</param>
        /// <param name="txtDepartmentID">部門 ID</param>
        /// <param name="txtEmployeeNo"></param>
        /// <param name="txtEmployeeName"></param>
        /// <param name="txtStatus">簽核狀態</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<PfaCycleEmp> GetQueryDatas(string txtPfaCycleID, string txtPfaOrgID, string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string txtStatus)
        {
            string[] status = { PfaCycleEmp_Status.NotSubmittedForApproval
                    , PfaCycleEmp_Status.InApprovalProcess
                    , PfaCycleEmp_Status.Approved
                    , PfaCycleEmp_Status.Locked };

            var result = GetAll().Where(x => status.Contains(x.Status));

            if (!string.IsNullOrEmpty(txtPfaCycleID))
            {
                var PfaCycleID = Guid.Parse(txtPfaCycleID);
                result = result.Where(x => x.PfaCycleID == PfaCycleID);
            }

            if (!string.IsNullOrEmpty(txtPfaOrgID))
            {
                var PfaOrgID = Guid.Parse(txtPfaOrgID);
                result = result.Where(x => x.PfaOrgID == PfaOrgID);
            }

            if (!string.IsNullOrEmpty(txtDepartmentID))
            {
                var departmentID = Guid.Parse(txtDepartmentID);
                result = result.Where(x => x.PfaDeptID == departmentID);
            }

            if (!string.IsNullOrEmpty(txtEmployeeNo))
                result = result.Where(x => x.Employees.EmployeeNO.Contains(txtEmployeeNo));

            if (!string.IsNullOrEmpty(txtEmployeeName))
                result = result.Where(x => x.Employees.EmployeeName.Contains(txtEmployeeName));

            if (!string.IsNullOrEmpty(txtStatus))
                result = result.Where(x => x.Status == txtStatus);

            return result
                .Include(x => x.Employees)
                .Include(x => x.PfaDept)
                .Include(x => x.PfaSignProcess)
                .ToList();
        }
    }
}