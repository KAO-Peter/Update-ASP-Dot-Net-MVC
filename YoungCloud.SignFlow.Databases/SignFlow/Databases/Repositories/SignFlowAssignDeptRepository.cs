using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class SignFlowAssignDeptRepository : SignFlowRepositoryBase<SignFlowAssignDept>
    {
        public SignFlowAssignDeptRepository()
            : base()
        {
        }
        public IQueryable<SignFlowAssignDept> GetDesignID(string fromType, string companyID, string deptID)
        {
            return base.GetAll().Where(x => x.CompanyID == companyID && (x.DeptID == "ALL" || x.DeptID == deptID) && x.FormType == fromType && x.IsUsed == "Y");
        }
    }
}
