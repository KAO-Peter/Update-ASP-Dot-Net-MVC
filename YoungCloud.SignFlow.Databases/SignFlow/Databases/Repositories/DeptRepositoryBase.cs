using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Databases.Repositories
{
    public abstract class DeptRepositoryBase<TEntity> : SignFlowRepositoryBase<TEntity>, IDeptRepositoryBase<TEntity> where TEntity : class
    {
        public abstract string GetManagerID(string deptID);
    }
}
