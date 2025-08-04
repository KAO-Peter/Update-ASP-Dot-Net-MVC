using System;

namespace HRPortal.Services.DDMC_PFA.Models
{
    /// <summary>
    /// 簽核
    /// </summary>
    public class PfaSignFlowDataViewModel
    {
        //表單簽核類別
        public Guid PfaSignTypeID { get; set; }

        //考核員工資料ID
        public Guid PfaCycleEmpID { get; set; }

        //考核部門ID
        public Guid? PfaDeptID { get; set; }

        //部門組織ID
        public Guid? PfaOrgID { get; set; }

        //員工ID
        public Guid EmployeeID { get; set; }

        // 是否代理自評
        public bool IsAgent { get; set; }

        // 建立者
        public Guid CreatedBy { get; set; }
    }
}
