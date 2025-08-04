using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class LimitViewModel
    {
        /// <summary>
        /// 自己不能看到主管對自己的評分
        /// </summary>
        /// <param name="managerIndicator">PfaCycleEmp.PfaEmpIndicator.ManagerIndicator</param>
        /// <param name="ceEmployeeID">PfaCycleEmp.EmployeeID</param>
        /// <param name="selfEmployeeID">Login EmployeeID</param>
        /// <returns></returns>
        public static double? ManagerIndicator(double? managerIndicator, Guid ceEmployeeID, Guid selfEmployeeID)
        {
            if (ceEmployeeID == selfEmployeeID)
            {
                return null;
            }
            else
            {
                return managerIndicator;
            }

        }

    }

}
