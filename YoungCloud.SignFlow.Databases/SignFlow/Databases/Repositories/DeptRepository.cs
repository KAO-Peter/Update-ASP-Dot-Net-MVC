using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public class DeptRepository : DeptRepositoryBase<SignFlowDesign>
    {
        public DeptRepository()
            : base()
        {
        }
        public override string GetManagerID(string deptID)
        {
            return this.GetAll().Where(x => x.SignDeptID==deptID).FirstOrDefault().SignerID;
        }
    }
}
